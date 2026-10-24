<!DOCTYPE html>
<html lang="{{ str_replace('_', '-', app()->getLocale()) }}">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <script src="/resources/js/jquery-2.2.4.min.js"></script>
    <script src="/resources/js/bootstrap.min.js"></script>
    <script src="/resources/js/nifty.min.js"></script>
    <script src="/resources/js/moment.min.js"></script>
    <script src="/resources/js/daterangepicker.min.js"></script>
    <script src="/resources/js/chart.js"></script>
    <link href="/resources/css/bootstrap.css" rel="stylesheet">
    <link href="/resources/css/bootstrap-icons.min.css" rel="stylesheet">
    <link href="/resources/css/bootstrap.min.css" rel="stylesheet">
    <link href="/resources/css/nifty.min.css" rel="stylesheet">
    <link href="/resources/css/magic-check.min.css" rel="stylesheet">
    <link href="/resources/css/daterangepicker.css" rel="stylesheet">
    <link href="/resources/css/miner.css" rel="stylesheet">
    <title>{{__('web_message.webSiteTitle')}}</title>
</head>
<body>
{{ $slot }}
</body>
</html>
