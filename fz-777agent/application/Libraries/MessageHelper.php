<?php
namespace App\Libraries;

/**
 *
 */
class MessageHelper
{
    public static function redirect($url)
    {
        header("Location:$url");
        exit();
    }
    public static function showMessage($container, $message, $goto = '', $wait = '', $referer = '')
    {
        $settings = $container->get('settings')['renderer'];
        if (isset($goto)) {
            $goto = APP_URL . $goto;
        }
        if (isset($referer)) {
            if (strpos($goto, '?') === false) {
                $goto = $goto . '?referer=' . $referer;
            } else {
                $goto = $goto . '&referer=' . $referer;
            }

        }
        $thisViewArgs = [
            'title'   => '系统提示',
            'message' => $message,
            'goto'    => $goto,
            'wait'    => $wait,
            //'referer' => $referer,
        ];
        include $settings['templatePath'] . 'message.helper';
        exit;
    }
}
