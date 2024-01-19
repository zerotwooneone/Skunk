# Skunk

## skunkapp

An angular SPA for the skunk application

### Development

To run (from skukapp directory)
> ng serve --open

### publish

> ng build --output-path ../publish/skunkapp

might need something like `--base-href /skunkapp/` if not hosted in wwwroot

### Setup notes

- Installed using NodeJs 20.11.0 LTS
- Angular 17.1.0

> ng new trustbuster --prefix=tb --routing --strict --view-encapsulation=ShadowDom --style=scss

> ng add @angular/pwa

> ng add @angular/material
