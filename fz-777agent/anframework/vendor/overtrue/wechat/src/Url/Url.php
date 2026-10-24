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
 * Url.php.
 *
 * @author    overtrue <user@example.invalid>
 * @copyright 2015 overtrue <user@example.invalid>
 *
 * @link      https://example.invalid/overtrue
 * @link      http://example.invalid
 */
namespace EasyWeChat\Url;

use EasyWeChat\Core\AbstractAPI;

/**
 * Class Url.
 */
class Url extends AbstractAPI
{
    const API_SHORTEN_URL = 'https://example.invalid/cgi-bin/shorturl';

    /**
     * Shorten the url.
     *
     * @param string $url
     *
     * @return string
     */
    public function shorten($url)
    {
        $params = [
                   'action' => 'long2short',
                   'long_url' => $url,
                  ];

        return $this->parseJSON('json', [self::API_SHORTEN_URL, $params]);
    }
}
