import { Component, NgZone, ViewEncapsulation, ViewChild, OnInit, NgModule } from '@angular/core';
import { MdSidenav, MdSidenavModule } from '@angular/material';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { ItemsService } from './items.service';

const SMALL_WIDTH_BREAKPOINT = 840;

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  encapsulation: ViewEncapsulation.None,
})
export class AppComponent implements OnInit {
  title = 'app';
  private mediaMatcher: MediaQueryList = matchMedia(`(max-width: ${SMALL_WIDTH_BREAKPOINT}px)`);

  constructor(public docItems: ItemsService,
    /*private _router: Router,*/
    zone: NgZone) {
    this.mediaMatcher.addListener(mql => zone.run(() => this.mediaMatcher = mql));
  }

  @ViewChild(MdSidenav) sidenav: MdSidenav;

  ngOnInit() {
    /*this._router.events.subscribe(() => {
      if (this.isScreenSmall()) {
        this.sidenav.close();
      }
    });*/
  }

  isScreenSmall(): boolean {
    return this.mediaMatcher.matches;
  }
}
