import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./app').then((m) => m.App),
  },
  {
    path: 'video/:videoId',
    loadComponent: () =>
      import('./components/vod-player/vod-player').then((m) => m.VodPlayerComponent),
  },
  {
    path: 'videos',
    loadComponent: () =>
      import('./components/video-list-page/video-list-page').then((m) => m.VideoListPageComponent),
  },
  { path: '**', redirectTo: '' },
];
