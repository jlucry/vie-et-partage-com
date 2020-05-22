/**
 * https://angular.io/guide/ngmodule#ngmodules
 * You launch the application by bootstrapping the AppModule in the main.ts file.
 * Angular offers a variety of bootstrapping options targeting multiple platforms.
 * This page describes two options, both targeting the browser.
 * https://angular.io/guide/ngmodule#compile-just-in-time-jit
 * https://angular.io/guide/ngmodule#compile-ahead-of-time-aot
 */
import 'hammerjs'

import { enableProdMode } from '@angular/core';
// The browser platform with a compiler.
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

// Import the root app.
import { AppModule } from './app/app.module';
// Import environment.
import { environment } from './environments/environment';

if (environment.production) {
  // Disable Angular's development mode, which turns off assertions and other checks within the framework.
  // https://angular.io/api/core/enableProdMode
  enableProdMode();
}

// Compile just-in-time (JIT):
// https://angular.io/guide/ngmodule#compile-just-in-time-jit
// Compile and launch the module.
platformBrowserDynamic().bootstrapModule(AppModule);

// For Compile ahead-of-time (AOT):
// https://angular.io/guide/ngmodule#compile-ahead-of-time-aot

// // The browser platform without a compiler
// import { platformBrowser } from '@angular/platform-browser';
//
// // The app module factory produced by the static offline compiler
// import { AppModuleNgFactory } from './app/app.module.ngfactory';
//
// // Launch with the app module factory.
// platformBrowser().bootstrapModuleFactory(AppModuleNgFactory);
