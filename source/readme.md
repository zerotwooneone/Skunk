# Skunk

## skunkapp

An angular SPA for the skunk application

### Development

To run (from skukapp directory)
> ng serve --open

To generate a new UI component
> ng generate component `componentName` --display-block

### publish

> ng build --output-path ../publish/skunkapp

might need something like `--base-href /skunkapp/` if not hosted in wwwroot

### Setup notes

- Installed using NodeJs 20.11.0 LTS
- Angular 17.1.0

> ng new trustbuster --prefix=tb --routing --strict --view-encapsulation=ShadowDom --style=scss

Add PWA so that the app can be installed like a desktop app
> ng add @angular/pwa

Add Material to get some UI elements
> ng add @angular/material

Add Environments to provide deployment specific variables (environment.ts and environment.development.ts)
> ng g environments

Add component that defaults to display:block
> ng g c home --display-block
