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
 * News.php.
 *
 * @author    overtrue <user@example.invalid>
 * @copyright 2015 overtrue <user@example.invalid>
 *
 * @link      https://example.invalid/overtrue
 * @link      http://example.invalid
 */
namespace EasyWeChat\Message;

/**
 * Class News.
 */
class News extends AbstractMessage
{
    /**
     * Message type.
     *
     * @var string
     */
    protected $type = 'news';

    /**
     * Properties.
     *
     * @var array
     */
    protected $properties = [
                                'title',
                                'description',
                                'url',
                                'image',
                            ];
    /**
     * Aliases of attribute.
     *
     * @var array
     */
    protected $aliases = [
        'image' => 'pic_url',
    ];
}
