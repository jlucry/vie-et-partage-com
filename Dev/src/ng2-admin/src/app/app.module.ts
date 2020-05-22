/** 
 * TypeScript Coding guidelines:
 * https://github.com/Microsoft/TypeScript/wiki/Coding-guidelines
 * 
 * Angular Style Guide:
 * https://angular.io/styleguide
 * 
 * NgModule:
 * https://angular.io/guide/ngmodule#ngmodules
 * 
 */

// The JavaScript import:
// https://angular.io/guide/bootstrapping#the-imports-array
// The JavaScript import statements give you access to symbols exported by other files so you 
// can reference them within this file. You add import statements to almost every application file.
// They have nothing to do with Angular and Angular knows nothing about them.
// --
// Like C++ include or c# using.
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import {
  MdAutocompleteModule,
  MdButtonModule,
  MdButtonToggleModule,
  MdCardModule,
  MdCheckboxModule,
  MdChipsModule,
  MdDatepickerModule,
  MdDialogModule,
  MdExpansionModule,
  MdFormFieldModule,
  MdGridListModule,
  MdIconModule,
  MdInputModule,
  MdListModule,
  MdMenuModule,
  MdNativeDateModule,
  MdPaginatorModule,
  MdProgressBarModule,
  MdProgressSpinnerModule,
  MdRadioModule,
  MdRippleModule,
  MdSelectModule,
  MdSidenavModule,
  MdSliderModule,
  MdSlideToggleModule,
  MdSnackBarModule,
  MdSortModule,
  MdTableModule,
  MdTabsModule,
  MdToolbarModule,
  MdTooltipModule,
  StyleModule,
} from '@angular/material';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { Routes, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { AppComponent } from './app.component';
import { ItemsService } from './items.service';
import { PostComponent } from './post/post.component';
import { HeaderComponent } from './header/header.component';
import { FooterComponent } from './footer/footer.component';


export const APP_ROUTES: Routes = [
  {path: '', component: PostComponent, pathMatch: 'full'},
  /*{
    path: 'categories',
    component: ComponentSidenav,
    children: [
      {path: '', component: ComponentCategoryList},
      {path: ':id', component: ComponentList},
    ],
  },
  {
    path: 'components',
    component: ComponentSidenav,
    children: [
      {path: '', component: ComponentCategoryList},
      {path: 'component/:id', redirectTo: ':id', pathMatch: 'full'},
      {path: 'category/:id', redirectTo: '/categories/:id', pathMatch: 'full'},
      {
        path: ':id',
        component: ComponentViewer,
        children: [
          {path: '', redirectTo: 'overview', pathMatch: 'full'},
          {path: 'overview', component: ComponentOverview, pathMatch: 'full'},
          {path: 'api', component: ComponentApi, pathMatch: 'full'},
          {path: 'examples', component: ComponentExamples, pathMatch: 'full'},
          {path: '**', redirectTo: 'overview'},
        ],
      },
    ],
  },
  {path: 'guides', component: GuideList},
  {path: 'guide/:id', component: GuideViewer},*/
  {path: '**', redirectTo: ''},
];

// https://angular.io/guide/architecture#modules
@NgModule({
  // The declarations array:
  // https://angular.io/guide/architecture#modules
  // Declare the view classes that belong to this module. Angular has three kinds of view classes: components, directives, and pipes.
  // --
  // https://angular.io/guide/bootstrapping#the-declarations-array
  // You tell Angular which components belong to the AppModule by listing it in the module's declarations array. 
  // As you create more components, you'll add them to declarations.
  // You must declare every component in an NgModule class. If you use a component without declaring it, you'll see a clear error message in the browser console.
  // Only declarables — components, directives and pipes — belong in the declarations array.
  // Do not put any other kind of class in declarations; not NgModule classes, not service classes, not model classes.
  declarations: [
    AppComponent,
    PostComponent,
    HeaderComponent,
    FooterComponent
  ],

  // The imports array:
  // https://angular.io/guide/architecture#modules
  // Other modules whose exported classes are needed by component templates declared in this module.
  // --
  // https://angular.io/guide/bootstrapping#the-imports-array
  // Add a module to the imports array when the application requires its features.
  // Only NgModule classes go in the imports array. Do not put any other kind of class in imports.
  // The module's imports array appears exclusively in the @NgModule metadata object. It tells Angular about 
  // specific other NgModules—all of them classes decorated with @NgModule—that the application needs to function properly.
  imports: [

  MdAutocompleteModule,
  MdButtonModule,
  MdButtonToggleModule,
  MdCardModule,
  MdCheckboxModule,
  MdChipsModule,
  MdDatepickerModule,
  MdDialogModule,
  MdExpansionModule,
  MdFormFieldModule,
  MdGridListModule,
  MdIconModule,
  MdInputModule,
  MdListModule,
  MdMenuModule,
  MdNativeDateModule,
  MdPaginatorModule,
  MdProgressBarModule,
  MdProgressSpinnerModule,
  MdRadioModule,
  MdRippleModule,
  MdSelectModule,
  MdSidenavModule,
  MdSliderModule,
  MdSlideToggleModule,
  MdSnackBarModule,
  MdSortModule,
  MdTableModule,
  MdTabsModule,
  MdToolbarModule,
  MdTooltipModule,
  StyleModule,
  
    MdSidenavModule,
    //RouterModule,
    RouterModule.forRoot(APP_ROUTES),
    BrowserAnimationsModule,
    BrowserModule
  ],

  // Declare service providers.
  // https://angular.io/guide/architecture#modules
  // Creators of services that this module contributes to the global collection of services; 
  // they become accessible in all parts of the app
  // https://angular.io/guide/architecture#dependency-injection
  // In brief, you must have previously registered a provider of the HeroService with the injector.
  // A provider is something that can create or return a service, typically the service class itself.
  // You can register providers in modules or in components.
  // In general, add providers to the root module so that the same instance of a service is available everywhere.
  // Alternatively, register at a component level in the providers property of the @Component metadata:
  // Registering at a component level means you get a new instance of the service with each new instance of that component.
  providers: [
    ItemsService
  ],

  // The bootstrap array:
  // https://angular.io/guide/architecture#modules
  // The main application view, called the root component, 
  // that hosts all other app views. Only the root module should set this bootstrap property
  // --
  // https://angular.io/guide/bootstrapping#the-bootstrap-array
  // You launch the application by bootstrapping the root AppModule. Among other things, the bootstrapping process 
  // creates the component(s) listed in the bootstrap array and inserts each one into the browser DOM.
  // Each bootstrapped component is the base of its own tree of components. Inserting a bootstrapped component usually 
  // triggers a cascade of component creations that fill out that tree.
  // While you can put more than one component tree on a host web page, that's not typical.
  // Most applications have only one component tree and they bootstrap a single root component.
  // You can call the one root component anything you want but most developers call it AppComponent.
  // --
  // The root component that Angular creates and inserts into the index.html host web page.
  bootstrap: [AppComponent]
})
export class AppModule { }
