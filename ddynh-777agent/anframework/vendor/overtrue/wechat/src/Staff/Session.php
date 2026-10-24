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
 * Session.php.
 *
 * @author    overtrue <user@example.invalid>
 * @copyright 2015 overtrue <user@example.invalid>
 *
 * @link      https://example.invalid/overtrue
 * @link      http://example.invalid
 */
namespace EasyWeChat\Staff;

use EasyWeChat\Core\AbstractAPI;

/**
 * Class Session.
 */
class Session extends AbstractAPI
{
    const API_CREATE = 'https://example.invalid/customservice/kfsession/create';
    const API_CLOSE = 'https://example.invalid/customservice/kfsession/close';
    const API_GET = 'https://example.invalid/customservice/kfsession/getsession';
    const API_LISTS = 'https://example.invalid/customservice/kfsession/getsessionlist';
    const API_WAITERS = 'https://example.invalid/customservice/kfsession/getwaitcase';

    /**
     * List all sessions of $account.
     *
     * @param string $account
     *
     * @return array
     */
    public function lists($account)
    {
        return $this->parseJSON('get', [self::API_LISTS, ['kf_account' => $account]]);
    }

    /**
     * List all waiters of $account.
     *
     * @return array
     */
    public function waiters()
    {
        return $this->parseJSON('get', [self::API_WAITERS]);
    }

    /**
     * Create a session.
     *
     * @param string $account
     * @param string $openid
     *
     * @return bool
     */
    public function create($account, $openId)
    {
        $params = [
                   'kf_account' => $account,
                   'openid' => $openId,
                  ];

        return $this->parseJSON('json', [self::API_CREATE, $params]);
    }

    /**
     * Close a session.
     *
     * @param string $account
     * @param string $openId
     *
     * @return bool
     */
    public function close($account, $openId)
    {
        $params = [
                   'kf_account' => $account,
                   'openid' => $openId,
                  ];

        return $this->parseJSON('json', [self::API_CLOSE, $params]);
    }

    /**
     * Get a session.
     *
     * @param string $openId
     *
     * @return bool
     */
    public function get($openId)
    {
        return $this->parseJSON('get', [self::API_GET, ['openid' => $openId]]);
    }
}
