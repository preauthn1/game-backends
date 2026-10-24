// This file is required by the index.html file and will
// be executed in the renderer process for that window.
// No Node.js APIs are available in this process because
// `nodeIntegration` is turned off. Use `preload.js` to
// selectively enable features needed in the rendering
// process.

const { ipcRenderer } = require('electron');

// document
//   .getElementById('myButton')
//   .addEventListener('click', () => {
//     ipcRenderer.send('click');
//   });


ipcRenderer.on('set-app-info', (event, arg) => { 
  window.appInfo = arg;
})

ipcRenderer.send('get-app-info-and-response-to-client', 'test')
