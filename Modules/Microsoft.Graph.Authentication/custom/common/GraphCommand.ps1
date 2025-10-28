# ------------------------------------------------------------------------------
#  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
# ------------------------------------------------------------------------------

Set-StrictMode -Version 2

function GraphCommand_ReadGraphCommandMetadata {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [string]$Path = (Join-Path $PSScriptRoot "MgCommandMetadata.json")
    )
    if (!(Test-Path $Path)) {
        throw "MgCommandMetadata file not found at $Path."
    }

    $Result = @{}
    try {
        Write-Debug "Reading MgCommandMetadata from file path - $Path."
        $FileProvider = [Microsoft.Graph.PowerShell.Authentication.Common.ProtectedFileProvider]::CreateFileProvider($Path, [Microsoft.Graph.PowerShell.Authentication.Common.FileProtection]::SharedRead)
        if ($PSEdition -eq "Core") {
            $Result = $FileProvider.CreateReader().ReadToEnd() | ConvertFrom-Json -AsHashtable
        }
        else {
            $DeserializationError = $null
            $Result = [Microsoft.Graph.PowerShell.Authentication.Helpers.StringUtil]::ConvertFromJson($FileProvider.CreateReader().ReadToEnd(), $true, 4, [ref] $DeserializationError)
            if ($null -ne $DeserializationError) { throw $DeserializationError }
        }
    }
    finally {
        $FileProvider.Dispose()
    }
    return $Result
}

function GraphCommand_ReadLegacyGraphCommandMapping {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [string]$Path = (Join-Path $PSScriptRoot "MgLegacyCommandMapping.json")    )
    if (!(Test-Path $Path)) {
        throw "MgLegacyCommandMapping file not found at $Path."
    }

    ## The following command will produce new json content for "MgLegacyCommandMapping.json" containing the latest cmdlet mappings from documentation.
    # Invoke-WebRequest 'https://learn.microsoft.com/en-us/powershell/microsoftgraph/azuread-msoline-cmdlet-map?view=graph-powershell-1.0' | Select-Object -ExpandProperty Content | Select-String -Pattern '<td>([a-z0-9-]+).*?</td>\s?<td><a href=.*?>([a-z0-9-]+).*?</a>(?:.*?<a href=.*?>([a-z0-9-]+).*?</a>)?(?:.*?<a href=.*?>([a-z0-9-]+).*?</a>)?' -AllMatches | Select-Object -ExpandProperty Matches | ForEach-Object { for ($i = 2; $i -lt $_.Groups.Count; $i++) { if ($_.Groups[$i].Index) { @{ LegacyMapping = $_.Groups[1].Value; Command = $_.Groups[$i].Value } } } } | Group-Object Command | ForEach-Object { [pscustomobject][ordered]@{ Command = $_.Name; LegacyMapping = [string[]]$_.Group.LegacyMapping } } | ConvertTo-Json
    ## The following command will produce new json content for "MgLegacyCommandMapping.json" containing the latest cmdlet mappings from documentation markdown.
    # Invoke-WebRequest 'https://github.com/MicrosoftDocs/microsoftgraph-docs-powershell/raw/main/microsoftgraph/docs-conceptual/azuread-msoline-cmdlet-map.md' | Select-Object -ExpandProperty Content | Select-String -Pattern '[|]([a-z0-9][a-z0-9-]+).*?\|\[([a-z0-9-]+)(?:.*?\[([a-z0-9-]+))?(?:.*?\[([a-z0-9-]+))?' -AllMatches | Select-Object -ExpandProperty Matches | ForEach-Object { for ($i = 2; $i -lt $_.Groups.Count; $i++) { if ($_.Groups[$i].Index) { @{ LegacyMapping = $_.Groups[1].Value; Command = $_.Groups[$i].Value } } } } | Group-Object Command | ForEach-Object { [pscustomobject][ordered]@{ Command = $_.Name; LegacyMapping = [string[]]$_.Group.LegacyMapping } } | ConvertTo-Json
    
    $Result = @{}
    try {
        Write-Debug "Reading MgLegacyCommandMapping from file path - $Path."
        $FileProvider = [Microsoft.Graph.PowerShell.Authentication.Common.ProtectedFileProvider]::CreateFileProvider($Path, [Microsoft.Graph.PowerShell.Authentication.Common.FileProtection]::SharedRead)
        if ($PSEdition -eq "Core") {
            $Result = $FileProvider.CreateReader().ReadToEnd() | ConvertFrom-Json -AsHashtable
        }
        else {
            $DeserializationError = $null
            $Result = [Microsoft.Graph.PowerShell.Authentication.Helpers.StringUtil]::ConvertFromJson($FileProvider.CreateReader().ReadToEnd(), $true, 4, [ref] $DeserializationError)
            if ($null -ne $DeserializationError) { throw $DeserializationError }
        }
    }
    finally {
        $FileProvider.Dispose()
    }
    return $Result
}
# SIG # Begin signature block
# MIIoRQYJKoZIhvcNAQcCoIIoNjCCKDICAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCB/LLlIv1DBRxZd
# uW4XT2SVo3wjVI1DgcnPLEcaqn8euKCCDXYwggX0MIID3KADAgECAhMzAAAEhV6Z
# 7A5ZL83XAAAAAASFMA0GCSqGSIb3DQEBCwUAMH4xCzAJBgNVBAYTAlVTMRMwEQYD
# VQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25p
# bmcgUENBIDIwMTEwHhcNMjUwNjE5MTgyMTM3WhcNMjYwNjE3MTgyMTM3WjB0MQsw
# CQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9u
# ZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMR4wHAYDVQQDExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIB
# AQDASkh1cpvuUqfbqxele7LCSHEamVNBfFE4uY1FkGsAdUF/vnjpE1dnAD9vMOqy
# 5ZO49ILhP4jiP/P2Pn9ao+5TDtKmcQ+pZdzbG7t43yRXJC3nXvTGQroodPi9USQi
# 9rI+0gwuXRKBII7L+k3kMkKLmFrsWUjzgXVCLYa6ZH7BCALAcJWZTwWPoiT4HpqQ
# hJcYLB7pfetAVCeBEVZD8itKQ6QA5/LQR+9X6dlSj4Vxta4JnpxvgSrkjXCz+tlJ
# 67ABZ551lw23RWU1uyfgCfEFhBfiyPR2WSjskPl9ap6qrf8fNQ1sGYun2p4JdXxe
# UAKf1hVa/3TQXjvPTiRXCnJPAgMBAAGjggFzMIIBbzAfBgNVHSUEGDAWBgorBgEE
# AYI3TAgBBggrBgEFBQcDAzAdBgNVHQ4EFgQUuCZyGiCuLYE0aU7j5TFqY05kko0w
# RQYDVR0RBD4wPKQ6MDgxHjAcBgNVBAsTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEW
# MBQGA1UEBRMNMjMwMDEyKzUwNTM1OTAfBgNVHSMEGDAWgBRIbmTlUAXTgqoXNzci
# tW2oynUClTBUBgNVHR8ETTBLMEmgR6BFhkNodHRwOi8vd3d3Lm1pY3Jvc29mdC5j
# b20vcGtpb3BzL2NybC9NaWNDb2RTaWdQQ0EyMDExXzIwMTEtMDctMDguY3JsMGEG
# CCsGAQUFBwEBBFUwUzBRBggrBgEFBQcwAoZFaHR0cDovL3d3dy5taWNyb3NvZnQu
# Y29tL3BraW9wcy9jZXJ0cy9NaWNDb2RTaWdQQ0EyMDExXzIwMTEtMDctMDguY3J0
# MAwGA1UdEwEB/wQCMAAwDQYJKoZIhvcNAQELBQADggIBACjmqAp2Ci4sTHZci+qk
# tEAKsFk5HNVGKyWR2rFGXsd7cggZ04H5U4SV0fAL6fOE9dLvt4I7HBHLhpGdE5Uj
# Ly4NxLTG2bDAkeAVmxmd2uKWVGKym1aarDxXfv3GCN4mRX+Pn4c+py3S/6Kkt5eS
# DAIIsrzKw3Kh2SW1hCwXX/k1v4b+NH1Fjl+i/xPJspXCFuZB4aC5FLT5fgbRKqns
# WeAdn8DsrYQhT3QXLt6Nv3/dMzv7G/Cdpbdcoul8FYl+t3dmXM+SIClC3l2ae0wO
# lNrQ42yQEycuPU5OoqLT85jsZ7+4CaScfFINlO7l7Y7r/xauqHbSPQ1r3oIC+e71
# 5s2G3ClZa3y99aYx2lnXYe1srcrIx8NAXTViiypXVn9ZGmEkfNcfDiqGQwkml5z9
# nm3pWiBZ69adaBBbAFEjyJG4y0a76bel/4sDCVvaZzLM3TFbxVO9BQrjZRtbJZbk
# C3XArpLqZSfx53SuYdddxPX8pvcqFuEu8wcUeD05t9xNbJ4TtdAECJlEi0vvBxlm
# M5tzFXy2qZeqPMXHSQYqPgZ9jvScZ6NwznFD0+33kbzyhOSz/WuGbAu4cHZG8gKn
# lQVT4uA2Diex9DMs2WHiokNknYlLoUeWXW1QrJLpqO82TLyKTbBM/oZHAdIc0kzo
# STro9b3+vjn2809D0+SOOCVZMIIHejCCBWKgAwIBAgIKYQ6Q0gAAAAAAAzANBgkq
# hkiG9w0BAQsFADCBiDELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24x
# EDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlv
# bjEyMDAGA1UEAxMpTWljcm9zb2Z0IFJvb3QgQ2VydGlmaWNhdGUgQXV0aG9yaXR5
# IDIwMTEwHhcNMTEwNzA4MjA1OTA5WhcNMjYwNzA4MjEwOTA5WjB+MQswCQYDVQQG
# EwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwG
# A1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSgwJgYDVQQDEx9NaWNyb3NvZnQg
# Q29kZSBTaWduaW5nIFBDQSAyMDExMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIIC
# CgKCAgEAq/D6chAcLq3YbqqCEE00uvK2WCGfQhsqa+laUKq4BjgaBEm6f8MMHt03
# a8YS2AvwOMKZBrDIOdUBFDFC04kNeWSHfpRgJGyvnkmc6Whe0t+bU7IKLMOv2akr
# rnoJr9eWWcpgGgXpZnboMlImEi/nqwhQz7NEt13YxC4Ddato88tt8zpcoRb0Rrrg
# OGSsbmQ1eKagYw8t00CT+OPeBw3VXHmlSSnnDb6gE3e+lD3v++MrWhAfTVYoonpy
# 4BI6t0le2O3tQ5GD2Xuye4Yb2T6xjF3oiU+EGvKhL1nkkDstrjNYxbc+/jLTswM9
# sbKvkjh+0p2ALPVOVpEhNSXDOW5kf1O6nA+tGSOEy/S6A4aN91/w0FK/jJSHvMAh
# dCVfGCi2zCcoOCWYOUo2z3yxkq4cI6epZuxhH2rhKEmdX4jiJV3TIUs+UsS1Vz8k
# A/DRelsv1SPjcF0PUUZ3s/gA4bysAoJf28AVs70b1FVL5zmhD+kjSbwYuER8ReTB
# w3J64HLnJN+/RpnF78IcV9uDjexNSTCnq47f7Fufr/zdsGbiwZeBe+3W7UvnSSmn
# Eyimp31ngOaKYnhfsi+E11ecXL93KCjx7W3DKI8sj0A3T8HhhUSJxAlMxdSlQy90
# lfdu+HggWCwTXWCVmj5PM4TasIgX3p5O9JawvEagbJjS4NaIjAsCAwEAAaOCAe0w
# ggHpMBAGCSsGAQQBgjcVAQQDAgEAMB0GA1UdDgQWBBRIbmTlUAXTgqoXNzcitW2o
# ynUClTAZBgkrBgEEAYI3FAIEDB4KAFMAdQBiAEMAQTALBgNVHQ8EBAMCAYYwDwYD
# VR0TAQH/BAUwAwEB/zAfBgNVHSMEGDAWgBRyLToCMZBDuRQFTuHqp8cx0SOJNDBa
# BgNVHR8EUzBRME+gTaBLhklodHRwOi8vY3JsLm1pY3Jvc29mdC5jb20vcGtpL2Ny
# bC9wcm9kdWN0cy9NaWNSb29DZXJBdXQyMDExXzIwMTFfMDNfMjIuY3JsMF4GCCsG
# AQUFBwEBBFIwUDBOBggrBgEFBQcwAoZCaHR0cDovL3d3dy5taWNyb3NvZnQuY29t
# L3BraS9jZXJ0cy9NaWNSb29DZXJBdXQyMDExXzIwMTFfMDNfMjIuY3J0MIGfBgNV
# HSAEgZcwgZQwgZEGCSsGAQQBgjcuAzCBgzA/BggrBgEFBQcCARYzaHR0cDovL3d3
# dy5taWNyb3NvZnQuY29tL3BraW9wcy9kb2NzL3ByaW1hcnljcHMuaHRtMEAGCCsG
# AQUFBwICMDQeMiAdAEwAZQBnAGEAbABfAHAAbwBsAGkAYwB5AF8AcwB0AGEAdABl
# AG0AZQBuAHQALiAdMA0GCSqGSIb3DQEBCwUAA4ICAQBn8oalmOBUeRou09h0ZyKb
# C5YR4WOSmUKWfdJ5DJDBZV8uLD74w3LRbYP+vj/oCso7v0epo/Np22O/IjWll11l
# hJB9i0ZQVdgMknzSGksc8zxCi1LQsP1r4z4HLimb5j0bpdS1HXeUOeLpZMlEPXh6
# I/MTfaaQdION9MsmAkYqwooQu6SpBQyb7Wj6aC6VoCo/KmtYSWMfCWluWpiW5IP0
# wI/zRive/DvQvTXvbiWu5a8n7dDd8w6vmSiXmE0OPQvyCInWH8MyGOLwxS3OW560
# STkKxgrCxq2u5bLZ2xWIUUVYODJxJxp/sfQn+N4sOiBpmLJZiWhub6e3dMNABQam
# ASooPoI/E01mC8CzTfXhj38cbxV9Rad25UAqZaPDXVJihsMdYzaXht/a8/jyFqGa
# J+HNpZfQ7l1jQeNbB5yHPgZ3BtEGsXUfFL5hYbXw3MYbBL7fQccOKO7eZS/sl/ah
# XJbYANahRr1Z85elCUtIEJmAH9AAKcWxm6U/RXceNcbSoqKfenoi+kiVH6v7RyOA
# 9Z74v2u3S5fi63V4GuzqN5l5GEv/1rMjaHXmr/r8i+sLgOppO6/8MO0ETI7f33Vt
# Y5E90Z1WTk+/gFcioXgRMiF670EKsT/7qMykXcGhiJtXcVZOSEXAQsmbdlsKgEhr
# /Xmfwb1tbWrJUnMTDXpQzTGCGiUwghohAgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMw
# EQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVN
# aWNyb3NvZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNp
# Z25pbmcgUENBIDIwMTECEzMAAASFXpnsDlkvzdcAAAAABIUwDQYJYIZIAWUDBAIB
# BQCgga4wGQYJKoZIhvcNAQkDMQwGCisGAQQBgjcCAQQwHAYKKwYBBAGCNwIBCzEO
# MAwGCisGAQQBgjcCARUwLwYJKoZIhvcNAQkEMSIEIP8Kj7xS+swCMgofQu9cNDoe
# ZS1dHor8dDFD5CMUZTDIMEIGCisGAQQBgjcCAQwxNDAyoBSAEgBNAGkAYwByAG8A
# cwBvAGYAdKEagBhodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20wDQYJKoZIhvcNAQEB
# BQAEggEAPooRcA4w5LGZmLohmK6N0nUshEfDN+0mHyXV1O2+CtD0rq6EaAAz7imy
# VnXJfTS4MJidKDP6xbBrnFJOnMjfvPtVC3GlbpCfICCtNsz08p/U0nfzq6JhRzSR
# LM3o44oZMSGO2cNzY9NDi5fF9XKKQnHUHLGK5X7SbXgmwtu+li821dUpU3+1Unos
# 7Y1osVhcfpAlS4chOpTM+66pp4n2QO0ELoZv7ByIMUb8AHVQsCGEpoIO6GdDshdD
# 4n6LgngWNS4IZO/wMMqFia5HE5s+qRR6HO7D+TNwm+tmAZecYp+O9gu7t07NW9UJ
# Qyft6q4HleYXN8aIy9Q+jLgfPtBBDqGCF68wgherBgorBgEEAYI3AwMBMYIXmzCC
# F5cGCSqGSIb3DQEHAqCCF4gwgheEAgEDMQ8wDQYJYIZIAWUDBAIBBQAwggFaBgsq
# hkiG9w0BCRABBKCCAUkEggFFMIIBQQIBAQYKKwYBBAGEWQoDATAxMA0GCWCGSAFl
# AwQCAQUABCC2i6sRL2qaTOxy7sSOs3PyDua9FX+rgbFIuaL9r8Dm/AIGaPJ8x9uc
# GBMyMDI1MTAxNzIzMzkwOS40OTFaMASAAgH0oIHZpIHWMIHTMQswCQYDVQQGEwJV
# UzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UE
# ChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMS0wKwYDVQQLEyRNaWNyb3NvZnQgSXJl
# bGFuZCBPcGVyYXRpb25zIExpbWl0ZWQxJzAlBgNVBAsTHm5TaGllbGQgVFNTIEVT
# Tjo1NTFBLTA1RTAtRDk0NzElMCMGA1UEAxMcTWljcm9zb2Z0IFRpbWUtU3RhbXAg
# U2VydmljZaCCEf0wggcoMIIFEKADAgECAhMzAAACG9CyuAJn93LPAAEAAAIbMA0G
# CSqGSIb3DQEBCwUAMHwxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9u
# MRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRp
# b24xJjAkBgNVBAMTHU1pY3Jvc29mdCBUaW1lLVN0YW1wIFBDQSAyMDEwMB4XDTI1
# MDgxNDE4NDgzMFoXDTI2MTExMzE4NDgzMFowgdMxCzAJBgNVBAYTAlVTMRMwEQYD
# VQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24xLTArBgNVBAsTJE1pY3Jvc29mdCBJcmVsYW5kIE9w
# ZXJhdGlvbnMgTGltaXRlZDEnMCUGA1UECxMeblNoaWVsZCBUU1MgRVNOOjU1MUEt
# MDVFMC1EOTQ3MSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBTZXJ2aWNl
# MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAjsWd52ZZkzB5Xe5g/l2G
# sOjAz30sg6jVxfFJV+w4xIDVyaI3LO8bIpmzYul3AZHg50UIQ8PrSRZGpQqFkRNu
# +o3YKJ4g2uGYBRksHnHYR0uVSCQg58ThkYyeplGX3oAvGRVuPIpQtAiTsR76A/gd
# oU7HDwEbb73bJwTyrbKHhR+WaMy9DQHI4k5Qo4+bZDs0kj76bvhJvdGU+S8zxQBp
# 7UAhjJnFqKxIusSITE7zCCR422ELhkhVVOFqK2w6h1MAvILe76hxRIcPj0SBL2r8
# O9tx5njU4+tg2rAdU153pmyhqazdpUccYBE9wDRFUd/e9CoWx7TdnUicB+Mai7RT
# 6qse7e5aGqX1B7bnj/ZHvrrfF+BJEIlS9iDXAUgekvXZ+FZmjvLwP+dN+0/crh++
# r4e8FknF7EX6IJfnmNeDN/68Z59kbaJ1f+P5mnKYfydCeZmxrGpS0taWkDk36D3j
# PVZflvxrc+1rhCIlM5v9agLEFI12QiBTfpOBOBr3AGCPk+eH0+latjQajug+2/BD
# 12qb82500LQytUWT2ota/HYnRgSv1jvZ0/dml1FsxWYzOnCrjfdB/7N6pNySt4vn
# +PGN6dFLim7kxos+B9WfQPezJi3fuKyyDAB9zSHPj1Zu8nZfecZJ9um4zj7DFgvJ
# XTDTnG5qlG4ZdbFRa/rrfzkCAwEAAaOCAUkwggFFMB0GA1UdDgQWBBS2vp93/lxL
# ppNK8OkauJ2AvNmIUDAfBgNVHSMEGDAWgBSfpxVdAF5iXYP05dJlpxtTNRnpcjBf
# BgNVHR8EWDBWMFSgUqBQhk5odHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpb3Bz
# L2NybC9NaWNyb3NvZnQlMjBUaW1lLVN0YW1wJTIwUENBJTIwMjAxMCgxKS5jcmww
# bAYIKwYBBQUHAQEEYDBeMFwGCCsGAQUFBzAChlBodHRwOi8vd3d3Lm1pY3Jvc29m
# dC5jb20vcGtpb3BzL2NlcnRzL01pY3Jvc29mdCUyMFRpbWUtU3RhbXAlMjBQQ0El
# MjAyMDEwKDEpLmNydDAMBgNVHRMBAf8EAjAAMBYGA1UdJQEB/wQMMAoGCCsGAQUF
# BwMIMA4GA1UdDwEB/wQEAwIHgDANBgkqhkiG9w0BAQsFAAOCAgEAZkU1XxQD4OTM
# 3GTht32TXShIfPBoMfSsFsBQqFOZqLJOxyJOllIBFpmpvOtGNPkC5Z8ldG8aCpvg
# FNo/jDWeT5FiW53dAj9KnZxpsQ3Pf5fRzSGHRcxEMOdXIVzDJwcZUX0cjfxna7yd
# Nv8eXB/Xk6G6SyrR2OH6S1LHMW11m3UvKF+eLjIPl45rximuDCoEd+ad0lOAXA5/
# vZOKN5n/ePYeP0LRchZX0Q6H8n/ZmSPMlbli3MO851Q09RmT/ZGHa+/Fdy+WLDrw
# cYykV9mUy/4TbwKw6FtdR6ZPHxMdIi1pk8Y2mC/GzCq0LCsH0uTFeQ6Q7Nc3MRmE
# R/3mLWUhbaWHgX1FbYchvR22b+Bup+YPR5Q/0BhaaAN6AIBfcGs+u/nJoIByyZKA
# 8cTyCmnUI/4vW6D4vywg3XBFf4f2DwFHy/evsC+58KMl+k2wa05X2kK0T/bCPLha
# ov9ZXyobawfNOLYGiauKT2FWvbwZzHIFCTxjBww6Pt5uRvCE/jnUcf/xhlOGMn6i
# KO9Xt49vZTE2SfIBk/34iLTRBJ6H7aGPTTQnza3OfWu1/dRycC6Wl5ons3PjnGXT
# SKSxXllJPmg6R/ulGonP/UCYoJ6mN+EXjfyDLPXLqsr91+VTG1rYzRCjPwBFAHv4
# EIwaE0ajCrf75eUGI3+oXU0UP6rloZ8wggdxMIIFWaADAgECAhMzAAAAFcXna54C
# m0mZAAAAAAAVMA0GCSqGSIb3DQEBCwUAMIGIMQswCQYDVQQGEwJVUzETMBEGA1UE
# CBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9z
# b2Z0IENvcnBvcmF0aW9uMTIwMAYDVQQDEylNaWNyb3NvZnQgUm9vdCBDZXJ0aWZp
# Y2F0ZSBBdXRob3JpdHkgMjAxMDAeFw0yMTA5MzAxODIyMjVaFw0zMDA5MzAxODMy
# MjVaMHwxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQH
# EwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xJjAkBgNV
# BAMTHU1pY3Jvc29mdCBUaW1lLVN0YW1wIFBDQSAyMDEwMIICIjANBgkqhkiG9w0B
# AQEFAAOCAg8AMIICCgKCAgEA5OGmTOe0ciELeaLL1yR5vQ7VgtP97pwHB9KpbE51
# yMo1V/YBf2xK4OK9uT4XYDP/XE/HZveVU3Fa4n5KWv64NmeFRiMMtY0Tz3cywBAY
# 6GB9alKDRLemjkZrBxTzxXb1hlDcwUTIcVxRMTegCjhuje3XD9gmU3w5YQJ6xKr9
# cmmvHaus9ja+NSZk2pg7uhp7M62AW36MEBydUv626GIl3GoPz130/o5Tz9bshVZN
# 7928jaTjkY+yOSxRnOlwaQ3KNi1wjjHINSi947SHJMPgyY9+tVSP3PoFVZhtaDua
# Rr3tpK56KTesy+uDRedGbsoy1cCGMFxPLOJiss254o2I5JasAUq7vnGpF1tnYN74
# kpEeHT39IM9zfUGaRnXNxF803RKJ1v2lIH1+/NmeRd+2ci/bfV+AutuqfjbsNkz2
# K26oElHovwUDo9Fzpk03dJQcNIIP8BDyt0cY7afomXw/TNuvXsLz1dhzPUNOwTM5
# TI4CvEJoLhDqhFFG4tG9ahhaYQFzymeiXtcodgLiMxhy16cg8ML6EgrXY28MyTZk
# i1ugpoMhXV8wdJGUlNi5UPkLiWHzNgY1GIRH29wb0f2y1BzFa/ZcUlFdEtsluq9Q
# BXpsxREdcu+N+VLEhReTwDwV2xo3xwgVGD94q0W29R6HXtqPnhZyacaue7e3Pmri
# Lq0CAwEAAaOCAd0wggHZMBIGCSsGAQQBgjcVAQQFAgMBAAEwIwYJKwYBBAGCNxUC
# BBYEFCqnUv5kxJq+gpE8RjUpzxD/LwTuMB0GA1UdDgQWBBSfpxVdAF5iXYP05dJl
# pxtTNRnpcjBcBgNVHSAEVTBTMFEGDCsGAQQBgjdMg30BATBBMD8GCCsGAQUFBwIB
# FjNodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpb3BzL0RvY3MvUmVwb3NpdG9y
# eS5odG0wEwYDVR0lBAwwCgYIKwYBBQUHAwgwGQYJKwYBBAGCNxQCBAweCgBTAHUA
# YgBDAEEwCwYDVR0PBAQDAgGGMA8GA1UdEwEB/wQFMAMBAf8wHwYDVR0jBBgwFoAU
# 1fZWy4/oolxiaNE9lJBb186aGMQwVgYDVR0fBE8wTTBLoEmgR4ZFaHR0cDovL2Ny
# bC5taWNyb3NvZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvTWljUm9vQ2VyQXV0XzIw
# MTAtMDYtMjMuY3JsMFoGCCsGAQUFBwEBBE4wTDBKBggrBgEFBQcwAoY+aHR0cDov
# L3d3dy5taWNyb3NvZnQuY29tL3BraS9jZXJ0cy9NaWNSb29DZXJBdXRfMjAxMC0w
# Ni0yMy5jcnQwDQYJKoZIhvcNAQELBQADggIBAJ1VffwqreEsH2cBMSRb4Z5yS/yp
# b+pcFLY+TkdkeLEGk5c9MTO1OdfCcTY/2mRsfNB1OW27DzHkwo/7bNGhlBgi7ulm
# ZzpTTd2YurYeeNg2LpypglYAA7AFvonoaeC6Ce5732pvvinLbtg/SHUB2RjebYIM
# 9W0jVOR4U3UkV7ndn/OOPcbzaN9l9qRWqveVtihVJ9AkvUCgvxm2EhIRXT0n4ECW
# OKz3+SmJw7wXsFSFQrP8DJ6LGYnn8AtqgcKBGUIZUnWKNsIdw2FzLixre24/LAl4
# FOmRsqlb30mjdAy87JGA0j3mSj5mO0+7hvoyGtmW9I/2kQH2zsZ0/fZMcm8Qq3Uw
# xTSwethQ/gpY3UA8x1RtnWN0SCyxTkctwRQEcb9k+SS+c23Kjgm9swFXSVRk2XPX
# fx5bRAGOWhmRaw2fpCjcZxkoJLo4S5pu+yFUa2pFEUep8beuyOiJXk+d0tBMdrVX
# VAmxaQFEfnyhYWxz/gq77EFmPWn9y8FBSX5+k77L+DvktxW/tM4+pTFRhLy/AsGC
# onsXHRWJjXD+57XQKBqJC4822rpM+Zv/Cuk0+CQ1ZyvgDbjmjJnW4SLq8CdCPSWU
# 5nR0W2rRnj7tfqAxM328y+l7vzhwRNGQ8cirOoo6CGJ/2XBjU02N7oJtpQUQwXEG
# ahC0HVUzWLOhcGbyoYIDWDCCAkACAQEwggEBoYHZpIHWMIHTMQswCQYDVQQGEwJV
# UzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UE
# ChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMS0wKwYDVQQLEyRNaWNyb3NvZnQgSXJl
# bGFuZCBPcGVyYXRpb25zIExpbWl0ZWQxJzAlBgNVBAsTHm5TaGllbGQgVFNTIEVT
# Tjo1NTFBLTA1RTAtRDk0NzElMCMGA1UEAxMcTWljcm9zb2Z0IFRpbWUtU3RhbXAg
# U2VydmljZaIjCgEBMAcGBSsOAwIaAxUAhoV6r49M4GBd41K1RYB1Z0f4zuCggYMw
# gYCkfjB8MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UE
# BxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSYwJAYD
# VQQDEx1NaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EgMjAxMDANBgkqhkiG9w0BAQsF
# AAIFAOyc+0MwIhgPMjAyNTEwMTcxNzI4MzVaGA8yMDI1MTAxODE3MjgzNVowdjA8
# BgorBgEEAYRZCgQBMS4wLDAKAgUA7Jz7QwIBADAJAgEAAgFOAgH/MAcCAQACAhKU
# MAoCBQDsnkzDAgEAMDYGCisGAQQBhFkKBAIxKDAmMAwGCisGAQQBhFkKAwKgCjAI
# AgEAAgMHoSChCjAIAgEAAgMBhqAwDQYJKoZIhvcNAQELBQADggEBAFsS7QQYGtnP
# VMGlXYKL7yasb7ZjN4UlJCJePs+LJN7eyo4cpQk9UICV0pf1aBXgXy03sQ6pQcOt
# i913JcNQT08VdhWewFAEvwxE3PRPNCIHPu5sqhAig72LvPT8A1LKiinJQG5pKYqn
# u48Owk6pDv/Fqiud9hMIjNNBht8hIobtrvS7zPLEw6QGw7Gt2umVn/gOFCg4enVf
# d9uTal7HoWN7Wb25mVu9YcnzyF+8IW+r2mUT1IMbXzVI8O08X72evooFar6wwxfb
# cfkWEL7eZFexvXuI+qAsQzQJoK+Hz+DJxzBEFnqLQ9U4BEJVYuJkQkbMuToNfcyc
# yEzkpGedSVExggQNMIIECQIBATCBkzB8MQswCQYDVQQGEwJVUzETMBEGA1UECBMK
# V2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0
# IENvcnBvcmF0aW9uMSYwJAYDVQQDEx1NaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0Eg
# MjAxMAITMwAAAhvQsrgCZ/dyzwABAAACGzANBglghkgBZQMEAgEFAKCCAUowGgYJ
# KoZIhvcNAQkDMQ0GCyqGSIb3DQEJEAEEMC8GCSqGSIb3DQEJBDEiBCB0j6l4R2nJ
# PumghcnnVAWEHwPdLRI2jKEMesX15jkinDCB+gYLKoZIhvcNAQkQAi8xgeowgecw
# geQwgb0EIDAlFJW4PaOYxxAIVd0u4kDAOlRU1nptzp18lTzdDYuAMIGYMIGApH4w
# fDELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1Jl
# ZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEmMCQGA1UEAxMd
# TWljcm9zb2Z0IFRpbWUtU3RhbXAgUENBIDIwMTACEzMAAAIb0LK4Amf3cs8AAQAA
# AhswIgQgnxtzhV/4wZvuAStZXZF2k0gPdNOleLKC15ksOq65QUAwDQYJKoZIhvcN
# AQELBQAEggIAhwPEHh8zthgj4pDPmT7f17ugVGneDqPMEulgSj5idx7z1kE7um+R
# bqkdcfW+CmfD09A2xD1ho+4iLjcay67rTLVHdryL4n8NLw5RM8yIlQ+YFjHWa2cK
# tLONw/sHMC6WIZGjXjetI6XbYOTWkWN2bHL24zB8NluArZYbNUlbFJ0XKbKyWRJc
# 0C7EunI76oYawlZ6gC1XSYDRP5FVUi0bTYx5srPH1sI6LyQMSykLvXZTWdSQjBJ4
# 6FCzMLfR4uotyfglk0+0HpMXHTQvHscIxEvKnRxggefuObQ/XpW56L9RoWTycafF
# Hl9ly6/xywI6EkJKzp6kZwcSajqp8qcNCjNMLwgf/lcg7IgZUHqgHEzhCJcUFTZ+
# yCGs/mxh80000pQWMYA4HdRhD9neKqMAUAWOXuPFEbUNO8NFmb6GEj+OstutsqsD
# JG/XAvi65rU3QXNzpqoRH7H4EK15vZ9dCJuMAf7+R8AOkGp9HHgzhmphzCTOnc7m
# PazOlnnn1fVsLM90cKFCHzn2oxZdeaszLa/Qq38MkPS/S6I425BLZMzmIa+4B0pB
# 8Kap6ojC9/l77cjC5XYVSTOOsqthA2/mqdZjoAd5fcqe7EtxcDcIMsMLhDrpQq5s
# joXuvRs3ZyeTX45sEnCdfhtRN0j3x4xvUtD5BkYqHUKKE37t4Cbs7FQ=
# SIG # End signature block
