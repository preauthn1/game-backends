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
 * Location.php.
 *
 * @author    overtrue <user@example.invalid>
 * @copyright 2015 overtrue <user@example.invalid>
 *
 * @link      https://example.invalid/overtrue
 * @link      http://example.invalid
 */
namespace EasyWeChat\Message;

/**
 * Class Location.
 */
class Location extends AbstractMessage
{
    /**
     * Message type.
     *
     * @var string
     */
    protected $type = 'location';

    /**
     * Properties.
     *
     * @var array
     */
    protected $properties = [
                             'latitude',
                             'longitude',
                             'scale',
                             'label',
                             'precision',
                            ];
}
