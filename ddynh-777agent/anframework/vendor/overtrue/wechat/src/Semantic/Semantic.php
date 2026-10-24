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
 * Semantic.php.
 *
 * @author    overtrue <user@example.invalid>
 * @copyright 2015 overtrue <user@example.invalid>
 *
 * @link      https://example.invalid/overtrue
 * @link      http://example.invalid
 */
namespace EasyWeChat\Semantic;

use EasyWeChat\Core\AbstractAPI;

/**
 * Class Semantic.
 */
class Semantic extends AbstractAPI
{
    const API_SEARCH = 'https://example.invalid/semantic/semproxy/search';

    /**
     * Get the semantic content of giving string.
     *
     * @param string       $keyword
     * @param array|string $categories
     * @param array        $other
     *
     * @return array
     */
    public function query($keyword, $categories, array $other = [])
    {
        $params = [
                   'query' => $keyword,
                   'category' => implode(',', (array) $categories),
                   'appid' => $this->getAccessToken()->getAppId(),
                  ];

        return $this->parseJSON('json', [self::API_SEARCH, array_merge($params, $other)]);
    }
}
