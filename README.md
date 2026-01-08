# README #

## Unity Model View ##

WebGL model for Heart Lung Machine.  Learning Objectives:

1. Parts familiarization
1. Learn CO2 Flushing principles and process

## Asset Organization ##

Original assets and references on:

* [Heart-Lung Machine Google Shared Drive](https://drive.google.com/drive/folders/0AHpT9Vj_MZZhUk9PVA) - FBX and dev sources. Requires membership to vpsimulation.com.
* [BCIT One Drive](https://bcit365-my.sharepoint.com/:f:/r/personal/vienna_ly_bcit_ca/Documents/_SOH/Heart%20Lung%20Machine/Reference%20Materials?csf=1&web=1&e=BcY0Zh) - Resources for reference shared by SMEs

## Features ##

TBD

## Dev ##

Using Unity Editor: 2022.3.49f1.  Based on MTV (Model Task Viewer) structure.

### Build & Player Settings ###

The web handler will use the **web repo** [`MTV_HeartLungMachine_web` framework](https://github.com/vie74050/MTV_HeartLungMachine_web)

* Platform: WebGL
* WebGL Template: webD2LTable
* Publish Settings: Build using gz compression, with and without decompression fallback checked

#### Build ####

1. Create a folder in `./Builds`. **Important!** The name of the folder will be used to name the build artefacts.
2. Using gz compression will generate the .gz files if deompression fallback unchecked
3. Copy the `.gz` files to the **web repo** `./uploads/Builds` folder before building the next filetype since it will be over-ridden
4. (optional if required non-compression fallback) Rebuild with decompression fallback checked to generate `.unityweb` files
5. Copy `.unityweb` files to the **web repo**`./uploads/Builds` folder

> NB: `[foldername].loader.js` created by Unity is not required as the **web repo** will already have a generic one.
> NB: `index.html` already existing in the **web repo**

### People ###

* Vienna - developer
* Jason Yu - modeller
* Tami Riley, Victoria Harris - SMEs
