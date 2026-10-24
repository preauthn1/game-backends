<!DOCTYPE html>
<html>
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  </head>
  <body>
    <script type="text/javascript">
      var isLoaded = false;
      var urlParams = new URLSearchParams(window.location.search);
      var hash = urlParams.get("hash");
      var userId = urlParams.get("userId");
      var username = urlParams.get("name");

      if (!hash || !userId || !username) {
        throw new Error("Missing required parameters");
      }

      var Tawk_API = Tawk_API || {},
        Tawk_LoadStart = new Date();
      (function () {
        var s1 = document.createElement("script"),
          s0 = document.getElementsByTagName("script")[0];
        s1.async = true;
        s1.src = "https://embed.tawk.to/6728d4744304e3196adce1bb/1ibrnrqec";
        s1.charset = "UTF-8";
        s1.setAttribute("crossorigin", "*");
        s0.parentNode.insertBefore(s1, s0);
      })();

      window.Tawk_API.onBeforeLoad = function () {
        window.Tawk_API.hideWidget();
      };

      window.Tawk_API.onLoad = function () {
        if (isLoaded) {
          return;
        }
        isLoaded = true;
        window.Tawk_API.login(
          {
            hash: hash,
            userId: userId,
            name: username,
          },
          function (error) {
            if (error) {
              console.error("Error logging in to Tawk:", error);
            } else {
              window.Tawk_API.maximize();
            }
          }
        );
      };

      window.Tawk_API.onChatMinimized = function () {
        if (window.flutter_inappwebview) {
          window.flutter_inappwebview.callHandler("sendToFlutter", {
            event: "CHAT_MINIMIZED",
          });
        }
      };
    </script>
  </body>
</html>
