<?php

/*
 * This file is part of the overtrue/wechat.
 *
 * (c) overtrue <user@example.invalid>
 *
 * This source file is subject to the MIT license that is bundled
 * with this source code in the file LICENSE.
 */

/**
 * helpers.php.
 *
 * @author    overtrue <user@example.invalid>
 * @copyright 2015 overtrue <user@example.invalid>
 *
 * @link      https://example.invalid/overtrue
 * @link      http://example.invalid
 */
namespace EasyWeChat\Payment;

/**
 * Generate a signature.
 *
 * @param array  $attributes
 * @param string $key
 * @param string $encryptMethod
 *
 * @return string
 */
function generate_sign(array $attributes, $key, $encryptMethod = 'md5')
{
    ksort($attributes);

    $attributes['key'] = $key;

    return strtoupper(call_user_func_array($encryptMethod, [urldecode(http_build_query($attributes))]));
}

/**
 * Get client ip address.
 *
 * @return string
 */
function get_client_ip()
{
    // for php-cli(phpunit etc.)
    if (!isset($_SERVER['REMOTE_ADDR'])) {
        return gethostbyname(gethostname());
    }

    return $_SERVER['REMOTE_ADDR'];
}
