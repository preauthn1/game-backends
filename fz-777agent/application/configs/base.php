<?php
#echo phpinfo();die;
return [
    'settings' => [
        'displayErrorDetails'               => false, // set to false in production

        // monolog日志配置
        'logger' => [
            'name' => '777',
            'level' => Monolog\Logger::DEBUG,
        ],

        // Renderer settings
        'renderer'                          => [
            'templatePath'      => APP_PATH . '/templates/2017/',
            'templateCachePath' => APP_PATH . '/caches/templates/',
        ],
        'determineRouteBeforeAppMiddleware' => false,
        'db'                                => [
            'driver'    => 'sqlsrv',
            'host'      => '0.0.0.0',
            'database'  => 'AdminSys',
            'username'  => 'gameuser',
            'password'  => '123456',
            'charset'   => 'utf8',
            'collation' => 'utf8_general_ci',
            'prefix'    => '',
        ],
        'log'                                => [
            'driver'    => 'sqlsrv',
            'host'      => '0.0.0.0',
            'database'  => 'LogDB',
            'username'  => 'gameuser',
            'password'  => '123456',
            'charset'   => 'utf8',
            'collation' => 'utf8_general_ci',
            'prefix'    => '',
        ],


        'center'                                => [
            'driver'    => 'sqlsrv',
            'host'      => '0.0.0.0',
            'database'  => 'CenterDB',
            'username'  => 'gameuser',
            'password'  => '123456',
            'charset'   => 'utf8',
            'collation' => 'utf8_general_ci',
            'prefix'    => '',
        ],

        'perGameLog'                                => [
            'driver'    => 'sqlsrv',
            'host'      => '0.0.0.0',
            'database'  => 'PerGameLog',
            'username'  => 'gameuser',
            'password'  => '123456',
            'charset'   => 'utf8',
            'collation' => 'utf8_general_ci',
            'prefix'    => '',
        ],

        'notify'                                => [
            'driver'    => 'sqlsrv',
            'host'      => '0.0.0.0',
            'database'  => 'Notify',
            'username'  => 'gameuser',
            'password'  => '123456',
            'charset'   => 'utf8',
            'collation' => 'utf8_general_ci',
            'prefix'    => '',
        ],

    ],
];
