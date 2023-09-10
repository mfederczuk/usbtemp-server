<!--
  Copyright (c) 2023 Michael Federczuk
  SPDX-License-Identifier: CC-BY-SA-4.0
-->

<!-- markdownlint-disable no-duplicate-heading -->

# Changelog #

All notable changes to this project will be documented in this file.
The format is based on [**Keep a Changelog v1.0.0**](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [**Semantic Versioning v2.0.0**](https://semver.org/spec/v2.0.0.html).

## [v0.1.0-indev05] - 2023-09-10 ##

[v0.1.0-indev05]: https://github.com/mfederczuk/usbtemp-server/releases/tag/v0.1.0-indev05

### Added ###

* The polling interval of the HTML polling page can be configured via the URL query parameter `pollingInterval`
* The displayed decimal digits of the HTML page can be configured via the URL query parameters `minDecimalDigits` and
  `maxDecimalDigits`

### Changed ###

* The default displayed decimal digits in the HTML page is now exactly 1

## [v0.1.0-indev04] - 2023-09-03 ##

[v0.1.0-indev04]: https://github.com/mfederczuk/usbtemp-server/releases/tag/v0.1.0-indev04

### Fixed ###

* Program would crash on startup if the virtual thermometers file does not exist

## v0.1.0-indev03 - 2023-09-03 [WITHDRAWN] ##

### Fixed ###

* ~~Program would crash on startup if the virtual thermometers file does not exist~~

## v0.1.0-indev02 - 2023-09-03 [WITHDRAWN] ##

### Added ###

* An automatic check for available updates of the program ([`de7cd22^...e726bc6`])
* "Virtual thermometers" — instead of using real, physical USB thermometers, these virtual thermometers can be used
  instead. ([`a5559c0^...769747c`])  
  They are mostly just supposed to be used for development and/or testing purposes
* Detection of connected USB thermometers. ([`06dc18b^...bf2f642`])
  If only a single device is detected, it will prompt to use that one, without needing to input the port name manually

[`de7cd22^...e726bc6`]: <https://github.com/mfederczuk/usbtemp-server/compare/de7cd2206cf4ba4c20d12c5551eb44eb72dc1d1c%5E...e726bc6f83f6dc9d70d6e961dfa0e0c88b0c78fa>
[`a5559c0^...769747c`]: <https://github.com/mfederczuk/usbtemp-server/compare/a5559c0f0263ead04211426f0c6581f6e07d7b0f%5E...769747cbea205aabd03c26416adff07ea5f85cfa>
[`06dc18b^...bf2f642`]: <https://github.com/mfederczuk/usbtemp-server/compare/06dc18b021441324169382b15f37dc6393b32a55%5E...bf2f642b95217fde9f90df237feceae6617edd2f>

## [v0.1.0-indev01] - 2023-07-25 ##

[v0.1.0-indev01]: https://github.com/mfederczuk/usbtemp-server/releases/tag/v0.1.0-indev01

Initial Release
