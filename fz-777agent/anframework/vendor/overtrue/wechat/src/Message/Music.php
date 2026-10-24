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
 * Music.php.
 *
 * @author    overtrue <user@example.invalid>
 * @copyright 2015 overtrue <user@example.invalid>
 *
 * @link      https://example.invalid/overtrue
 * @link      http://example.invalid
 */
namespace EasyWeChat\Message;

/**
 * Class Music.
 *
 * @property string $url
 * @property string $hq_url
 * @property string $title
 * @property string $description
 * @property string $thumb_media_id
 * @property string $format
 */
class Music extends AbstractMessage
{
    /**
     * Message type.
     *
     * @var string
     */
    protected $type = 'music';

    /**
     * Properties.
     *
     * @var array
     */
    protected $properties = [
                             'title',
                             'description',
                             'url',
                             'hq_url',
                             'thumb_media_id',
                             'format',
                            ];
}
