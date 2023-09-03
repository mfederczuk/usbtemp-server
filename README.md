<!--
  Copyright (c) 2023 Michael Federczuk
  SPDX-License-Identifier: CC-BY-SA-4.0
-->

# usbtemp-server #

[version_shield]: https://img.shields.io/badge/version-0.1.0--indev04-informational.svg
[release_page]: https://github.com/mfederczuk/usbtemp-server/releases/tag/v0.1.0-indev04 "Release v0.1.0-indev04"
[![version: 0.1.0-indev04][version_shield]][release_page]
[![Changelog](https://img.shields.io/badge/-Changelog-informational.svg)](CHANGELOG.md "Changelog")

## About ##

**usbtemp-server** is a simple server application that reads the temperature from
a [DS18B20]-compatible, 1-Wire-connected USB thermometer and exposes that functionality as a HTTP JSON API.

This software is specifically written to support the thermometers sold by [usbtemp.com],
though — in theory — any DS18B20-compatible device should work. (this project is **not** sponsored by usbtemp.com)

It also serves a HTML page that continuously polls the server for the temperature and displays it.  
It's designed to be used an OBS Browser Source.

> sub to [twitch.tv/helmahof]

[DS18B20]: <https://www.analog.com/en/products/ds18b20.html> "DS18B20 Datasheet and Product Info | Analog Devices"
[usbtemp.com]: <https://usbtemp.com> "Digital Thermometer −55 — +125°C with USB connection"
[twitch.tv/helmahof]: <https://twitch.tv/helmahof> "Helmahof - Twitch"

## Usage ##

Visit the page <https://mfederczuk.github.io/usbtemp-server> for the usage guide.

## Download ##

Fully built and self-contained (you *don't* need .NET installed on your system) artifacts for
both Linux-based and Microsoft Windows systems (both for x86, 64-bit architecture) can be downloaded from
the [release page][release_page].

## Contributing ##

Read through the [Contribution Guidelines](CONTRIBUTING.md) if you want to contribute to this project.

## License ##

**usbtemp-server** is licensed under both the [**Mozilla Public License 2.0**](LICENSES/MPL-2.0.txt) AND
the [**Apache License 2.0**](LICENSES/Apache-2.0.txt).  
For more information about copying and licensing, see the [`COPYING.txt`](COPYING.txt) file.

_(note that this project is **not** affiliated with usbtemp.com)_
