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
 * Reply.php.
 *
 * @author    overtrue <user@example.invalid>
 * @copyright 2015 overtrue <user@example.invalid>
 *
 * @link      https://example.invalid/overtrue
 * @link      http://example.invalid
 */
namespace EasyWeChat\Reply;

use EasyWeChat\Core\AbstractAPI;

/**
 * Class Reply.
 */
class Reply extends AbstractAPI
{
    const API_GET_CURRENT_SETTING = 'https://example.invalid/cgi-bin/get_current_autoreply_info';

    /**
     * Get current auto reply settings.
     *
     * @return \EasyWeChat\Support\Collection
     */
    public function current()
    {
        return $this->parseJSON('get', [self::API_GET_CURRENT_SETTING]);
    }
}
