<?php

namespace App\Services;

use App\Enums\AgentCustomizedSettingsEnum as AgentCustomizedSettingsEnum;
use App\Services\AgentService;

/**
 * Class AppService.
 */
class AppService extends BaseService
{
    public function getInfo($params)
    {
        $params->merge([
            'setting_keys' => [
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
            'android_url' => $settings[AgentCustomizedSettingsEnum::ANDROID_DOWNLOAD_URL] ?? '',
            'ios_url' => $settings[AgentCustomizedSettingsEnum::IOS_DOWNLOAD_URL] ?? '',
            'android_min_version' => $settings[AgentCustomizedSettingsEnum::ANDROID_MIN_VERSION] ?? '1.0.0',
            'ios_min_version' => $settings[AgentCustomizedSettingsEnum::IOS_MIN_VERSION] ?? '1.0.0',
        ];
        return $data;
    }
}
