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
 * Staff.php.
 *
 * @author    overtrue <user@example.invalid>
 * @copyright 2015 overtrue <user@example.invalid>
 *
 * @link      https://example.invalid/overtrue
 * @link      http://example.invalid
 */
namespace EasyWeChat\Staff;

use EasyWeChat\Core\AbstractAPI;
use EasyWeChat\Support\Collection;

/**
 * Class Staff.
 */
class Staff extends AbstractAPI
{
    const API_LISTS = 'https://example.invalid/cgi-bin/customservice/getkflist';
    const API_ONLINE = 'https://example.invalid/cgi-bin/customservice/getonlinekflist';
    const API_DELETE = 'https://example.invalid/customservice/kfaccount/del';
    const API_UPDATE = 'https://example.invalid/customservice/kfaccount/update';
    const API_CREATE = 'https://example.invalid/customservice/kfaccount/add';
    const API_INVITE_BIND = 'https://example.invalid/customservice/kfaccount/inviteworker';
    const API_MESSAGE_SEND = 'https://example.invalid/cgi-bin/message/custom/send';
    const API_AVATAR_UPLOAD = 'https://example.invalid/customservice/kfaccount/uploadheadimg';
    const API_RECORDS = 'https://example.invalid/customservice/msgrecord/getrecord';

    /**
     * List all staffs.
     *
     * @return \EasyWeChat\Support\Collection
     */
    public function lists()
    {
        return $this->parseJSON('get', [self::API_LISTS]);
    }

    /**
     * List all online staffs.
     *
     * @return \EasyWeChat\Support\Collection
     */
    public function onlines()
    {
        return $this->parseJSON('get', [self::API_ONLINE]);
    }

    /**
     * Create a staff.
     *
     * @param string $account
     * @param string $nickname
     *
     * @return \EasyWeChat\Support\Collection
     */
    public function create($account, $nickname)
    {
        $params = [
                   'kf_account' => $account,
                   'nickname' => $nickname,
                  ];

        return $this->parseJSON('json', [self::API_CREATE, $params]);
    }

    /**
     * Update a staff.
     *
     * @param string $account
     * @param string $nickname
     *
     * @return \EasyWeChat\Support\Collection
     */
    public function update($account, $nickname)
    {
        $params = [
                   'kf_account' => $account,
                   'nickname' => $nickname,
                  ];

        return $this->parseJSON('json', [self::API_UPDATE, $params]);
    }

    /**
     * Delete a staff.
     *
     * @param string $account
     *
     * @return \EasyWeChat\Support\Collection
     */
    public function delete($account)
    {
        // XXX: 微信那帮搞技术的都 TM 是 SB，url上的文本居然不 TM urlencode,
        // 这里客服账号因为有 @ 符，而微信不接收urlencode的账号。。
        // 简直是日了...
        // #222
        // PS: 如果你是微信做接口的，奉劝你们，尊重技术，不会别乱搞，笨不是你们的错，你们出来坑人就是大错特错。
        $accessTokenField = sprintf('%s=%s', $this->accessToken->getQueryName(), $this->accessToken->getToken());
        $url = sprintf(self::API_DELETE.'?%s&kf_account=%s', $accessTokenField, $account);

        $contents = $this->getHttp()->parseJSON(file_get_contents($url));

        $this->checkAndThrow($contents);

        return new Collection($contents);
    }

    /**
     * Invite a staff.
     *
     * @param string $account
     * @param string $wechatId
     *
     * @return \EasyWeChat\Support\Collection
     */
    public function invite($account, $wechatId)
    {
        $params = [
                   'kf_account' => $account,
                   'invite_wx' => $wechatId,
                  ];

        return $this->parseJSON('json', [self::API_INVITE_BIND, $params]);
    }

    /**
     * Set staff avatar.
     *
     * @param string $account
     * @param string $path
     *
     * @return \EasyWeChat\Support\Collection
     */
    public function avatar($account, $path)
    {
        return $this->parseJSON('upload', [self::API_AVATAR_UPLOAD, ['media' => $path], [], ['kf_account' => $account]]);
    }

    /**
     * Get message builder.
     *
     * @param \EasyWeChat\Message\AbstractMessage|string $message
     *
     * @return \EasyWeChat\Staff\MessageBuilder
     *
     * @throws \EasyWeChat\Core\Exceptions\InvalidArgumentException
     */
    public function message($message)
    {
        $messageBuilder = new MessageBuilder($this);

        return $messageBuilder->message($message);
    }

    /**
     * Send a message.
     *
     * @param string|array $message
     *
     * @return \EasyWeChat\Support\Collection
     */
    public function send($message)
    {
        return $this->parseJSON('json', [self::API_MESSAGE_SEND, $message]);
    }

    /**
     * Get staff session history.
     *
     * @param int $startTime
     * @param int $endTime
     * @param int $page
     * @param int $pageSize
     *
     * @return \EasyWeChat\Support\Collection
     */
    public function records($startTime, $endTime, $page = 1, $pageSize = 10)
    {
        $params = [
                   'starttime' => is_numeric($startTime) ? $startTime : strtotime($startTime),
                   'endtime' => is_numeric($endTime) ? $endTime : strtotime($endTime),
                   'pageindex' => $page,
                   'pagesize' => $pageSize,
                  ];

        return $this->parseJSON('json', [self::API_RECORDS, $params]);
    }
}
