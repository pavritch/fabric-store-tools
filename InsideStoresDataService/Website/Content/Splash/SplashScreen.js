function onSourceDownloadProgressChanged(sender, eventArgs)
{
    if (eventArgs.progress)
    {
        var pct = Math.floor((Math.round(eventArgs.progress * 100) / 5)) * 5;
        sender.findName("rectBar").Width = eventArgs.progress * sender.findName("rectBorder").Width;
        sender.findName("pctCompleteText").Text = "" + pct;
    }
    else
    {
        // Firefox
        var pct2 = Math.floor((Math.round(eventArgs.get_progress() * 100) / 5)) * 5;
        sender.findName("rectBar").Width = eventArgs.get_progress() * sender.findName("rectBorder").Width;
        sender.findName("pctCompleteText").Text = "" + pct2;
    }
}
