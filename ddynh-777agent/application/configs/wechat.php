<?php
return [
    'debug'   => false,
    'app_id'  => 'wx9bdf50142f63bb3a', // AppID
    'secret'  => 'b9f79207b45c8b200586720258a9f1b7', // AppSecret
    'token'   => 'csguoruantz', // Token
    'aes_key' => 'P63DKjLISyANakGKtAyJnGsmDmrISvSSSod04CCN85K', // EncodingAESKey，安全模式下请一定要填写！！！
    'log'     => [
        'level' => 'error',
        'file'  => APP_PATH . '/logs/wechat/easywechat.log',
    ],
    'oauth'   => [
        'scopes'   => ['snsapi_base'], //snsapi_base snsapi_userinfo
        'callback' => 'callback',
    ],
    'payment' => [
        'merchant_id' => '1483978052',
        'key'         => '12345678912345678912345678912345',
        //'cert_path'   => 'path/to/your/cert.pem', // XXX: 绝对路径！！！！
        //'key_path'    => 'path/to/your/key', // XXX: 绝对路径！！！！
        // 'device_info'     => '013467007045764',
        // 'sub_app_id'      => '',
        // 'sub_merchant_id' => '',
        // ...
    ],
    'guzzle'  => [
        'timeout' => 3.0, // 超时时间（秒）
        //'verify' => false, // 关掉 SSL 认证（强烈不建议！！！）
    ],
];
