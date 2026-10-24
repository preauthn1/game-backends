<?php

namespace App\Libraries;

use Monolog\Handler\StreamHandler;

class logger
{
    private static $ins = null;

    public static function getIns() {
        if (self::$ins == null) {
            $settings = require APP_PATH . '/configs/base.php';
            $logger = new \Monolog\Logger($settings['settings']['logger']['name']);
            $path = APP_ROOT . '/logs/' . date('Y-m-d') . 'app.log';
            $fileHandler = new StreamHandler($path, $settings['settings']['logger']['level']);
            $logger->pushHandler($fileHandler);
            self::$ins = $logger;
        }
        return self::$ins;
    }
}