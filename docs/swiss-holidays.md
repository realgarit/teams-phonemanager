# Swiss holiday rules

Verified against official sources on 2026-10-14. Future years assume unchanged law.

## Product boundary

The picker offers canton-wide statutory dates and requires a workplace region for Aargau,
Freiburg and Solothurn. Every final phone closure schedule must be reviewed with the customer.

- **Statutory**: federal 1 August and canton-designated Sunday-equivalent holidays under Art. 20a ArG.
- **Additional cantonal rest day**: separately legislated dates outside that list. Included by default,
  clearly labelled, and removable as a group before adding. These can still carry cantonal restrictions;
  being outside Art. 20a does not mean unrestricted work is allowed.
- **Sunday observance**: existing named Sunday celebrations, not additional Art. 20a holidays.
- **Customer-specific / unclassified**: manually entered or adjusted dates and older/imported entries
  without classification metadata. Never infer legal status from a holiday name alone.

Municipal patronal days, customary days off, administrative staff calendars, sector-specific shop
closures and company holidays are not automatically treated as canton-wide statutory holidays.
In particular, Zurich Knabenschiessen, Sechseläuten and 2 January remain customer-specific.
Zug retains only its nine statutory dates. No dates are automatically moved just because they fall
on a weekend. Weekend statutory dates themselves are retained.

## Corrected date rules

- Glarus Näfelser Fahrt is the first Thursday in April, postponed seven days when that Thursday
  falls in Holy Week. In 2026 the correct date is **9 April**, not 2 April.
  [Cantonal government announcement](https://www.gl.ch/public-newsroom.html/31/newsroomnews/14237/title/n%C3%A4felser-fahrt).
- AI and AR Stephanstag is omitted when 26 December falls on Tuesday or Saturday. This is
  the same date condition expressed as three consecutive rest days in AI and Christmas on
  Monday/Friday in AR. It excludes 2026 and 2028 within the current 2025–2030 range.
- Geneva LJF Art. 1(2) substitution applies to employers outside the federal Labour Act.
  The general picker adds no substitute Mondays. Customer-specific handling must consider all
  qualifying Sunday dates, including 31 December across a year boundary.
- Neuchâtel has a different rule: retain 2 January / 26 December when the previous date is Sunday.
- Solothurn 1 May starts at **12:00**, in both legal regions.

## Complete regional selection

Freiburg uses stable domain codes `fr-catholic` and `fr-reformed`. These describe the **legal
territory of the workplace**, not the customer's religion. The reformed list applies to Courgevaux,
Fräschels, Greng, Kerzers, Meyriez, Muntelier, Murten, Ried bei Kerzers, Ulmiz, Mont-Vully and
**the localities Flamatt and Sensebrügg** within Wünnewil-Flamatt. The rest of Wünnewil-Flamatt
uses the Catholic list. Both selections contain nine statutory dates including 1 August.

Solothurn uses `so-bucheggberg` (six dates, including the half-day) and `so-other` (nine dates,
including the half-day). Outside Bucheggberg add Fronleichnam, Assumption and All Saints.

Aargau retains six displayed legal groups, including the Bergdietikon and Rheinfelden municipal
exceptions. Current EG ArR § 6 has no old Tuesday/Saturday exclusion for 2 January/Stephanstag;
do not copy that historic exception from the repealed 961.111 ordinance.

An absent or unknown region remains explicitly incomplete at the domain boundary. The UI requires
an actual supported choice, resets it on canton changes, and guards the command as well as the button.

## Source register

The [SECO overview](https://www.seco.admin.ch/dam/de/sd-web/x5LtHPJdNis5/Anhang-Feiertage-Schweiz-Auszug-ArG-ArGV1-5-SECO-SB-2024-DE.pdf)
is a cross-check; current canton law takes precedence. Do not use the Federal Office of Justice
procedural-deadline list as an employment-law calendar.

| Canton | Official source / provision | Additional cantonal rest days in the picker (excluding Sundays) |
|---|---|---|
| AG | [EG ArR § 6; current version 1 September 2025](https://gesetzessammlungen.ag.ch/app/de/texts_of_law/961.200) | None |
| AI | [Ruhetagsgesetz Art. 2(b)/(c)](https://ai.clex.ch/app/de/texts_of_law/822.200) | Mariä Himmelfahrt, Allerheiligen, Mariä Empfängnis |
| AR | [Arbeitsverordnung Art. 7](https://ar.clex.ch/app/de/texts_of_law/822.11) | None |
| BE | [Feiertagsgesetz Art. 2; cross-checked with SECO](https://www.belex.sites.be.ch/app/de/texts_of_law/555.1) | None |
| BL | [Ruhetagsgesetz § 2; cross-checked with SECO](https://bl.clex.ch/app/de/texts_of_law/547) | None |
| BS | [RLG § 2; cross-checked with SECO](https://www.gesetzessammlung.bs.ch/app/de/texts_of_law/811.100) | None |
| FR | [Cantonal labour authority: Catholic and Reformed territories](https://www.fr.ch/de/arbeit-und-unternehmen/arbeitnehmer/feiertage) | None |
| GE | [LJF Art. 1(1)/(2)](https://silgeneve.ch/legis/data/rsg_j1_45.htm) | None |
| GL | [Labour inspectorate: categories 1–3](https://www.gl.ch/verwaltung/volkswirtschaft-und-inneres/wirtschaft-und-arbeit/arbeit/arbeitsinspektorat/feiertagstagsregelung-sonntagsverkaeufe.html/999) | Pfingstmontag |
| GR | [Ruhetagsgesetz Art. 2/3; cross-checked with SECO](https://www.gr-lex.gr.ch/app/de/texts_of_law/520.100) | None |
| JU | [Law 555.1 Art. 3/4, in force 1 January 2023](https://rsju.jura.ch/fr/viewdocument.html?download=1&id=38948&idn=20105) | Berchtoldstag, Unabhängigkeitstag Jura, Mariä Himmelfahrt, Allerheiligen |
| LU | [Law 855 § 1a(1)/(3); current version 27 September 2026](https://srl.lu.ch/app/de/texts_of_law/855) | Mariä Empfängnis |
| NE | [Official calendar: conditional 2 January and 26 December](https://www.ne.ch/themes/economie-et-emploi/jours-feries-officiels) | None |
| NW | [Ruhetagsgesetz Art. 2(1)/(2)](https://gesetze.nw.ch/app/de/texts_of_law/921.1) | Josefstag |
| OW | [Ruhetagsgesetz Art. 2(1)/(3)](https://gdb.ow.ch/app/de/texts_of_law/975.2) | Bruderklausenfest |
| SG | [RLG Art. 2/3; cross-checked with SECO](https://www.gesetzessammlung.sg.ch/app/de/texts_of_law/552.1) | None |
| SH | [Ruhetagsgesetz Art. 1/2; cross-checked with SECO](https://rechtsbuch.sh.ch/app/de/texts_of_law/900.200) | None |
| SO | [Arbeitsgesetz-Verordnung § 12; 1 May from 12:00; Bucheggberg exception](https://bgs.so.ch/api/de/versions/649/pdf_file) | None |
| SZ | [Labour inspectorate: sections 1–3](https://www.sz.ch/public/upload/assets/36693/Feiertagsregelung_Kanton_Schwyz.pdf) | Dreikönigstag, Ostermontag, Pfingstmontag, Mariä Empfängnis, Stephanstag |
| TG | [Labour inspectorate: 1 May is a cantonal rest day; new Ruhetagsgesetz effective 2026 also checked](https://awa.tg.ch/public/upload/assets/190722/ArZ-Merkblatt_Feier-_und_Ruhetage_2024_-_2026_-_Version_f%C3%BCr_HP.pdf?fp=1) | Tag der Arbeit |
| TI | [LALL 843.100 Art. 6(a)/(b)](https://m3.ti.ch/CAN/RLeggi/public/index.php/raccolta-leggi/legge/num/569) | San Giuseppe, Tag der Arbeit, Pfingstmontag, Fronleichnam, San Pietro e Paolo, Mariä Empfängnis |
| UR | [Labour authority: separate ArG and LSG columns; LSG Art. 9 also checked](https://www.ur.ch/_docn/404812/%C3%9Cbersicht_Feiertage_Kanton_Uri_gem._ArG_und_LSG.pdf) | Dreikönigstag, Josefstag, Ostermontag, Pfingstmontag, Stephanstag |
| VD | [DGEM employment-law FAQ p. 32, referring to LEmp Art. 47](https://www.vd.ch/fileadmin/user_upload/themes/economie_emploi/emploi/fichiers_pdf/FAQ-droit_du_travail-DGEM.pdf) | None |
| VS | [Ordonnance cantonale sur le travail Art. 7](https://lex.vs.ch/app/fr/texts_of_law/822.100) | None |
| ZG | [Cantonal labour authority: eight canton holidays plus 1 August](https://zg.ch/de/wirtschaft-arbeit/arbeitsbedingungen/arbeits-und-ruhezeiten) | None |
| ZH | [Cantonal labour inspectorate: statutory and non-statutory holidays](https://www.zh.ch/de/wirtschaft-arbeit/arbeitsbedingungen/arbeitsssicherheit-gesundheitsschutz/arbeits-ruhezeiten/feiertage.html) | None |

## Persistence and compatibility

`HolidayCategory` is preserved through the UI provider, saved configuration and tenant-as-code
round trips. Portable schema-v1 entries accept an optional `category`; its absence means Custom.
Older strict readers may reject a newly exported file containing this field: import it with this
release or newer. Unknown enum values are rejected. Classification is descriptive metadata, not
a verified assertion about imported customer data. Editing a date/time/range clears its automatic
classification. Cancelling an edit or the whole manager preserves the original values and category.

No Graph/PowerShell script builder or authentication flow is changed. No tenant calendar is
silently migrated: previously created Teams schedules must be reviewed and updated by the operator.

## Regression evidence

`SwissHolidayCatalogTests` checks independently transcribed 2026 statutory dates for all 26
cantons, all ten additional-rest-day sets, all six AG groups, both FR and SO regions, the Glarus
Holy Week exception, both Appenzell conditions, Geneva/Neuchâtel substitution differences, and
weekend/category invariants for 2025–2030. `HolidaySelectionTests` checks command validation,
preview/filter behaviour, region reset, duplicate prevention, classification round trips and cancel/edit
behaviour. The screenshot harness includes English/German empty, regional, mixed-category,
long Aargau dropdown and saved-list states; run with `SCREENSHOT_FILTER=holiday-audit-`.

Representative inspected renders: [Freiburg](screenshots/holiday-audit-fr-reformed-de.png),
[Solothurn](screenshots/holiday-audit-so-other-de.png),
[additional rest days](screenshots/holiday-audit-ti-en.png),
and [saved list](screenshots/holiday-audit-manager-de.png).
