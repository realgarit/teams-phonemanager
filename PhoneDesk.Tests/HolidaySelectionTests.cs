using PhoneDesk.Holidays;
using PhoneDesk.Models;
using PhoneDesk.Portability;
using PhoneDesk.Services;
using PhoneDesk.Services.Holidays;
using PhoneDesk.Tests.TestSupport;
using PhoneDesk.ViewModels;
using System.Text.Json;

namespace PhoneDesk.Tests;

public class HolidaySelectionTests
{
    private static VariablesViewModel Create(ViewModelTestHarness h) => new(
        h.PowerShellContextService.Object, h.PowerShellCommandService.Object, h.LoggingService.Object,
        h.SessionManager.Object, h.NavigationService.Object, h.ErrorHandlingService.Object,
        h.ValidationService.Object, h.SharedStateService.Object, h.DialogService.Object);

    [Theory]
    [InlineData("Fribourg", 0, 9)]
    [InlineData("Fribourg", 1, 9)]
    [InlineData("Solothurn", 0, 6)]
    [InlineData("Solothurn", 1, 9)]
    public void RegionalChoiceIsRequiredAndApplied(string canton, int region, int count)
    {
        var h = new ViewModelTestHarness();
        var vm = Create(h);
        vm.SelectedCanton = canton;
        vm.SelectedYear = 2026;
        vm.ShowPredefinedHolidaysWizard = true;
        Assert.False(vm.CanApplyPredefinedSelection);
        vm.ApplyPredefinedHolidaysCommand.Execute(null);
        Assert.Empty(vm.Variables.HolidaySeries);
        Assert.True(vm.ShowPredefinedHolidaysWizard);

        vm.SelectedHolidayRegionIndex = region;
        Assert.True(vm.CanApplyPredefinedSelection);
        Assert.Equal(count, vm.PredefinedHolidayPreview.Count(x => x.IsStatutory));
        vm.ApplyPredefinedHolidaysCommand.Execute(null);
        Assert.Equal(count, vm.Variables.HolidaySeries.Count(x => x.IsStatutory));
        Assert.False(vm.ShowPredefinedHolidaysWizard);
    }

    [Fact]
    public void ChangingCantonClearsRegionAndStalePreview()
    {
        var vm = Create(new ViewModelTestHarness());
        vm.SelectedCanton = "Fribourg";
        vm.SelectedHolidayRegionIndex = 1;
        Assert.NotEmpty(vm.PredefinedHolidayPreview);
        vm.SelectedCanton = "Solothurn";
        Assert.Equal(-1, vm.SelectedHolidayRegionIndex);
        Assert.Empty(vm.PredefinedHolidayPreview);
        Assert.False(vm.CanApplyPredefinedSelection);
        vm.SelectedHolidayRegionIndex = 10;
        Assert.False(vm.CanApplyPredefinedSelection);
    }

    [Fact]
    public void AdditionalRestDaysAreLabelledOptionalAndNotDuplicated()
    {
        var vm = Create(new ViewModelTestHarness());
        vm.SelectedCanton = "Tessin";
        vm.SelectedYear = 2026;
        Assert.Equal(9, vm.PredefinedHolidayPreview.Count(h => h.IsStatutory));
        Assert.Equal(6, vm.PredefinedHolidayPreview.Count(h => h.IsAdditionalCantonalRestDay));
        vm.IncludeAdditionalCantonalRestDays = false;
        Assert.Equal(9, vm.PredefinedHolidayPreview.Count);
        vm.ApplyPredefinedHolidaysCommand.Execute(null);
        vm.IncludeAdditionalCantonalRestDays = true;
        vm.ApplyPredefinedHolidaysCommand.Execute(null);
        vm.ApplyPredefinedHolidaysCommand.Execute(null);
        Assert.Equal(15, vm.Variables.HolidaySeries.Count);
        Assert.Equal(6, vm.Variables.HolidaySeries.Count(h => h.IsAdditionalCantonalRestDay));
    }

    [Fact]
    public void CancelManagerRestoresNamesRangesAndClassification()
    {
        var vm = Create(new ViewModelTestHarness());
        var holiday = new HolidayEntry(new DateTime(2026, 5, 1), TimeSpan.Zero, "Tag der Arbeit",
            new DateTime(2026, 5, 2), TimeSpan.Zero) { Category = HolidayCategory.AdditionalCantonalRestDay };
        vm.Variables.HolidaySeries.Add(holiday);
        vm.OpenHolidaySeriesManagerCommand.Execute(null);
        vm.Variables.HolidaySeries.Clear();
        vm.CancelHolidaySeriesManagerCommand.Execute(null);
        var restored = Assert.Single(vm.Variables.HolidaySeries);
        Assert.Equal(holiday.Name, restored.Name);
        Assert.Equal(holiday.EndDateTime, restored.EndDateTime);
        Assert.Equal(holiday.Category, restored.Category);
    }

    [Fact]
    public void DateEditsOnlyApplyOnSaveAndBecomeCustomerSpecific()
    {
        var vm = Create(new ViewModelTestHarness());
        var holiday = new SwissHolidaysProvider().GetHolidays("Switzerland", "Zug", null, 2026).First();
        vm.Variables.HolidaySeries.Add(holiday);
        var original = holiday.Date;
        vm.EditHolidayCommand.Execute(holiday);
        vm.DialogHolidayDate = original.AddDays(3);
        vm.CancelEditHolidayCommand.Execute(null);
        Assert.Equal(original, holiday.Date);
        Assert.True(holiday.IsStatutory);
        vm.EditHolidayCommand.Execute(holiday);
        vm.DialogHolidayDate = original.AddDays(3);
        vm.SaveEditHolidayCommand.Execute(null);
        Assert.Equal(original.AddDays(3), holiday.Date);
        Assert.True(holiday.IsCustom);
    }

    [Fact]
    public void LocalJsonPreservesCategoryAndLegacyEntriesRemainUnclassified()
    {
        var holiday = new HolidayEntry(new DateTime(2026, 5, 1), TimeSpan.Zero, "Tag der Arbeit")
            { Category = HolidayCategory.AdditionalCantonalRestDay };
        var restored = JsonSerializer.Deserialize<HolidayEntry>(JsonSerializer.Serialize(holiday))!;
        Assert.True(restored.IsAdditionalCantonalRestDay);
        Assert.True(JsonSerializer.Deserialize<HolidayEntry>("{\"Date\":\"2026-05-01T00:00:00\"}")!.IsCustom);
    }

    [Fact]
    public void PortableConfigurationPreservesCategoriesAndRejectsInvalidValues()
    {
        var vm = Create(new ViewModelTestHarness());
        vm.Variables.HolidaySeries.Add(new HolidayEntry(new DateTime(2026, 5, 1), TimeSpan.Zero)
            { Category = HolidayCategory.AdditionalCantonalRestDay });
        var service = new TenantAsCodeService();
        var document = PhoneManagerVariablesPortabilityMapper.ToDocument(vm.Variables);
        var json = service.SerializeConfiguration(document);
        var roundTrip = PhoneManagerVariablesPortabilityMapper.FromDocument(service.DeserializeConfiguration(json));
        Assert.True(Assert.Single(roundTrip.HolidaySeries).IsAdditionalCantonalRestDay);
        var badHoliday = document.Configuration.HolidayTemplate with
        {
            Series = document.Configuration.HolidayTemplate.Series.Select(h => h with { Category = (HolidayCategory)99 }).ToArray()
        };
        Assert.Throws<InvalidDataException>(() => service.SerializeConfiguration(document with
        {
            Configuration = document.Configuration with { HolidayTemplate = badHoliday }
        }));
    }
}
