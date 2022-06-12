<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Inside Stores Control Panel</title>
    <style type="text/css">
        html, body
        {
            height: 100%;
            overflow: auto;
        }
        body
        {
            padding: 0;
            margin: 0;
        }
        #silverlightControlHost
        {
            overflow-y:hidden;
            overflow-x:auto;
        }
        #installRequired
        {
            width: 600px;
            margin-top: 40px;
            margin-left: auto;
            margin-right: auto;
            padding: 20pt;
        }
        h2
        {
            font-family: Arial;
            font-size: 10pt;
            font-weight: bold;
            color: #606060;
        }
        p
        {
            font-family: Trebuchet MS,Arial;
            font-size: 9pt;
            color: #303030;
        }
        h1 {font-family:Arial;font-size:18pt;color:#808080}
    </style>

    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js"></script>

    <script type="text/javascript" src="Scripts/Silverlight.js"></script>
    <script type="text/javascript" src="Content/Splash/SplashScreen.js"></script>

    <script type="text/javascript">
        function onSilverlightError(sender, args) {
            var appSource = "";
            if (sender != null && sender != 0) {
                appSource = sender.getHost().Source;
            }

            var errorType = args.ErrorType;
            var iErrorCode = args.ErrorCode;

            if (errorType == "ImageError" || errorType == "MediaError") {
                return;
            }

            var errMsg = "Unhandled Error in Silverlight Application " + appSource + "\n";

            errMsg += "Code: " + iErrorCode + "    \n";
            errMsg += "Category: " + errorType + "       \n";
            errMsg += "Message: " + args.ErrorMessage + "     \n";

            if (errorType == "ParserError") {
                errMsg += "File: " + args.xamlFile + "     \n";
                errMsg += "Line: " + args.lineNumber + "     \n";
                errMsg += "Position: " + args.charPosition + "     \n";
            }
            else if (errorType == "RuntimeError") {
                if (args.lineNumber != 0) {
                    errMsg += "Line: " + args.lineNumber + "     \n";
                    errMsg += "Position: " + args.charPosition + "     \n";
                }
                errMsg += "MethodName: " + args.methodName + "     \n";
            }

            throw new Error(errMsg);
        }
    </script>


    <script type="text/javascript" charset="utf-8">
        function adjustSize() {

            var newWindowWidth = $(window).width();
//            if (newWindowWidth < 1325)
//                newWindowWidth = 1325;
            $("#silverlightControlHost").width(newWindowWidth);

            var newWindowHeight = $(window).height();
//            if (newWindowHeight < 800)
//                newWindowHeight = 800;
            $("#silverlightControlHost").height(newWindowHeight);
        }

        $(window).ready(function () {
            adjustSize();
        });

        $(window).resize(function () {
            adjustSize();
        });

    </script>

</head>
<body>
        
        <div id="silverlightControlHost">
        <object data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="100%" height="100%" >
		  <param name="source" value="ClientBin/ControlPanel.xap"/>
		  <param name="onError" value="onSilverlightError" />
		  <param name="windowless" value="false" />
		  <param name="splashscreensource" value="Content/Splash/SplashScreen.xaml" />
          <param name="enablegpuacceleration" value="true" />
		  <param name="onSourceDownloadProgressChanged" value="onSourceDownloadProgressChanged" />
		  <param name="background" value="white" />
          <param name="minRuntimeVersion"value="4.0.60310.0" />

			  <param name="autoUpgrade" value="true" />

            <div id="installRequired">
                <div style="text-align:center;">
                    <h1>Inside Stores Control Panel</h1>
                </div>
                <div style="margin-left:50px;margin-right:50px;margin-top:30pt">
                <h2>Please Install Silverlight</h2>
                <p>The control panel used to manage the Inside Stores data service requires the Silverlight browser plug-in. Please click the button
                below to install.</p>
                </div>
                <div style="text-align: center; margin-top: -10">
                    <a  href="http://go.microsoft.com/fwlink/?LinkID=149156&amp;v=4.0.60310.0" style="text-decoration: none">
                        <img src="http://go.microsoft.com/fwlink/?LinkId=161376" alt="Get Microsoft Silverlight" style="border-style: none" />
                    </a>
                </div>
            </div>
	    </object><iframe id="_sl_historyFrame" style="visibility:hidden;height:0px;width:0px;border:0px"></iframe>
        </div>

</body>
</html>
