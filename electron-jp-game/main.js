// Modules to control application life and create native browser window
const {
  app,  
  ipcMain,
  BrowserWindow
} = require('electron')
const path = require('path')
const os = require('os');

function createWindow() {
  let isDev = process.env.NODE_ENV === 'dev' || process.env.NODE_ENV === 'dev ';

  const computerUserName = os.hostname();

  //判斷是否是開發人員的電腦
  const isDevComputer = computerUserName == 'DESKTOP-EMCG6HU';
  const isEnableDebugTool = isDevComputer || isDev;

  const mainWindow = new BrowserWindow({

    //width: 1280,
    //height: 900,
    resize: true,
    autoHideMenuBar: !isEnableDebugTool,
    icon: __dirname + '/assets/img/pgs_256.ico',
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      webviewTag: true,      
      nodeIntegration: true,
      contextIsolation: false,
      devTools: isEnableDebugTool
    },
  })

  //視窗展開到最大
  mainWindow.maximize();

  // and load the index.html of the app.
  mainWindow.loadFile('index.html')
}



function XXXXXX() {

  // let isDev = process.env.NODE_ENV === 'dev' || process.env.NODE_ENV === 'dev ';

  // // Create the browser window.
  // const mainWindow = new BrowserWindow({
  //   width: 1280,
  //   height: 900,
  //   resize: true,
  //   autoHideMenuBar: isDev ? false : true,
  //   icon: __dirname + '/assets/img/pgs_256.ico',
  //   webPreferences: {
  //     preload: path.join(__dirname, 'preload.js'),
  //     webviewTag: true,      
  //     nodeIntegration: true,
  //     contextIsolation: false,
  //     devTools: isDev ? true : false
  //   },
  // })

  // // and load the index.html of the app.
  // mainWindow.loadFile('index.html')

}



// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.whenReady().then(() => {
  createWindow()

  app.on('activate', function () {
    // On macOS it's common to re-create a window in the app when the
    // dock icon is clicked and there are no other windows open.
    if (BrowserWindow.getAllWindows().length === 0) createWindow()
  })
})

// Quit when all windows are closed, except on macOS. There, it's common
// for applications and their menu bar to stay active until the user quits
// explicitly with Cmd + Q.
app.on('window-all-closed', function () {
  if (process.platform !== 'darwin') app.quit()
})


//pgs方 自訂的一些事件
ipcMain.on('get-app-info-and-response-to-client', (event, arg) => {
  const appInfo = 
  {
     version : app.getVersion()
  };
  event.reply('set-app-info', appInfo);
})

