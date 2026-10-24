<?php

namespace App\Services;

use App\Enums\AgentCustomizedSettingsEnum as AgentCustomizedSettingsEnum;
use App\Services\AgentService;

/**
 * Class Template.
 */
class TemplateService extends BaseService
{
    public function getTemplate($params)
    {
        $params->merge([
            'setting_keys' => [
                AgentCustomizedSettingsEnum::SMS_VERIFY,
                AgentCustomizedSettingsEnum::CAPTCHA_VERIFY,
                AgentCustomizedSettingsEnum::WITHDRAWAL_REQUIRES_VERIFIED_ID,
                AgentCustomizedSettingsEnum::PURCHASE_MINER_REQUIRES_VERIFIED_ID,
                AgentCustomizedSettingsEnum::RENT_MINER_REQUIRES_VERIFIED_ID,
                AgentCustomizedSettingsEnum::GROUP_MINER_REQUIRES_VERIFIED_ID,
                AgentCustomizedSettingsEnum::ANDROID_DOWNLOAD_URL,
                AgentCustomizedSettingsEnum::IOS_DOWNLOAD_URL,
                AgentCustomizedSettingsEnum::ANDROID_MIN_VERSION,
                AgentCustomizedSettingsEnum::IOS_MIN_VERSION,
            ]
        ]);
        $agentService = new AgentService();
        $result = $agentService->getCustomizedSettings($params);
        $settings = $result->pluck('value', 'setting_key');
        $data = [
            'sms_verify' => $settings[AgentCustomizedSettingsEnum::SMS_VERIFY] ?? '0',
            'captcha_verify' => $settings[AgentCustomizedSettingsEnum::CAPTCHA_VERIFY] ?? '0',
            'withdrawal_requires_verified_id' => $settings[AgentCustomizedSettingsEnum::WITHDRAWAL_REQUIRES_VERIFIED_ID] ?? '1',
            'purchase_miner_requires_verified_id' => $settings[AgentCustomizedSettingsEnum::PURCHASE_MINER_REQUIRES_VERIFIED_ID] ?? '1',
            'rent_miner_requires_verified_id' => $settings[AgentCustomizedSettingsEnum::RENT_MINER_REQUIRES_VERIFIED_ID] ?? '1',
            'group_miner_requires_verified_id' => $settings[AgentCustomizedSettingsEnum::GROUP_MINER_REQUIRES_VERIFIED_ID] ?? '1',
            'android_url' => $settings[AgentCustomizedSettingsEnum::ANDROID_DOWNLOAD_URL] ?? '',
            'ios_url' => $settings[AgentCustomizedSettingsEnum::IOS_DOWNLOAD_URL] ?? '',
            'android_min_version' => $settings[AgentCustomizedSettingsEnum::ANDROID_MIN_VERSION] ?? '1.0.0',
            'ios_min_version' => $settings[AgentCustomizedSettingsEnum::IOS_MIN_VERSION] ?? '1.0.0',
        ];
        return $data;
    }
}
