function initialWebviewUrl() {

  console.log(window.appInfo);

  let url = getUrlByApi();
  var webview = document.getElementById('mywebview');
  webview.loadURL(url);

}


function getUrlByApi() {

  let ts = + new Date();
  let url = "https://example.invalid/cdn-betmonsters-com-cdn-bucket-online/jp_environment.json?v=" + ts;

  try
  {
      var xmlHttp = new XMLHttpRequest();
      xmlHttp.open("GET", url, false); // false for synchronous request
      xmlHttp.send(null);
    
      let resObject = JSON.parse(xmlHttp.responseText);
      console.log(xmlHttp.responseText);
    
      if(resObject && resObject.domains && resObject.domains.length > 0)
      {
          return resObject.domains[0];
      }
  }
  catch(e)
  {
    console.log('---error---');
    console.log(e);
  }

  return "https://example.invalid/login";
}


function hideOverlay() {

  var element = document.getElementById('loading-overlay');
  element.style.display = 'none';
}

function showVersion() {

  var element = document.getElementById('fixVersion');
  element.style.display = 'block';
  element.innerHTML = "v" + window.appInfo.version;

}

