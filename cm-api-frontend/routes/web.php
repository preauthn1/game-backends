<?php

use Illuminate\Support\Facades\Route;
use Illuminate\Support\Facades\File;
use App\Helpers\Common;

Route::get('/', function () {
    //Log::info('Welcome page visited  -------');
    return response()->json([
        'ip' => request()->ip(),
        'userAgent' => request()->userAgent(),
        'acceptLanguage' => request()->header('Accept-Language'),
        'timestamp' => now()->toDateTimeString()
    ]);
});

Route::get('/tawk', function () {
    Log::info('tawk page visited  -------');
    return view('tawk')->withHeaders([
        'Content-Security-Policy' => "frame-ancestors 'self' http://localhost:* https://localhost:* https://example.invalid https://example.invalid",
        'X-Frame-Options' => 'ALLOW-FROM https://example.invalid'
    ]);
});

Route::get('/ssq', function () {
    Log::info('ssq page visited  -------');
    return view('ssq')->withHeaders([
        'Content-Security-Policy' => "frame-ancestors 'self' http://localhost:* https://localhost:* https://example.invalid https://example.invalid",
        'X-Frame-Options' => 'ALLOW-FROM https://example.invalid'
    ]);
});

Route::get('/infos', function () {
    $path = base_path('version.json');
    $ip = Common::getUserIp() ?? '';
    $approvedIps = '0.0.0.0,0.0.0.0';
    $result = ['ip' => $ip];
    if (File::exists($path) && (in_array($ip, explode(',', $approvedIps)))) {
        $json = File::get($path);
        $data = json_decode($json, true);
        if (!$data) $data = ['message' => json_last_error_msg()];
        $result = array_merge($result, $data);
    }
    return response()->json($result);
});