<?php

namespace App\Enums;

enum MenuEnum
{
    public static function getMenuList($lang): array
    {
        $menuList = [
            'dashboard' => [
                'title' => [
                    'en' => 'Dashboard',
                    'cn' => '数据统计',
                    'tw' => '數據統計',
                ],
                'dashboard' => [
                    'en' => 'Dashboard',
                    'cn' => '仪表板',
                    'tw' => '儀表板',
                ]
            ],
            'system' => [
                'title' => [
                    'en' => 'System Mgmt',
                    'cn' => '系统管理',
                    'tw' => '系統管理',
                ],
                'menuMgmt' => [
                    'en' => 'Menu Mgmt',
                    'cn' => '选单管理',
                    'tw' => '選單管理',
                ],
                'addMenu' => [
                    'en' => 'Add Menu',
                    'cn' => '新增选单',
                    'tw' => '新增選單',
                ],
                'modifyMenu' => [
                    'en' => 'Modify Menu',
                    'cn' => '选单修改',
                    'tw' => '選單修改',
                ],
                'groupDefine' => [
                    'en' => 'Group Define',
                    'cn' => '组别定义',
                    'tw' => '組別定義',
                ],
                'depositSetting' => [
                    'en' => 'Deposit Setting',
                    'cn' => '充值设置',
                    'tw' => '充值設置',
                ],
                'depositAccountSetting' => [
                    'en' => 'Deposit Account Setting',
                    'cn' => '收款账号设置',
                    'tw' => '收款帳號設置',
                ],
                'withdrawalSetting' => [
                    'en' => 'Withdrawal Setting',
                    'cn' => '提现设置',
                    'tw' => '提現設置',
                ],
                'mlm' => [
                    'en' => 'MLM',
                    'cn' => '代理分销设置',
                    'tw' => '代理分銷設置',
                ],
                'invitePromotion' => [
                    'en' => 'Invite Promotion',
                    'cn' => '邀请活动设置',
                    'tw' => '邀請活動設置',
                ],
                'promotionSetting' => [
                    'en' => 'Promotion Setting',
                    'cn' => '活动设置',
                    'tw' => '活動設置',
                ],
                'addPromotion' => [
                    'en' => 'Add Promotion',
                    'cn' => '新增活动',
                    'tw' => '新增活動',
                ],
                'modifyPromotion' => [
                    'en' => 'Modify Promotion',
                    'cn' => '活动修改',
                    'tw' => '活動修改',
                ],
                'promotionDetail' => [
                    'en' => 'Promotion Detail',
                    'cn' => '活动详情',
                    'tw' => '活動詳情',
                ],
                'siteMessage' => [
                    'en' => 'Site Message',
                    'cn' => '站内信',
                    'tw' => '站內信',
                ],
                'announcement' => [
                    'en' => 'Announcement',
                    'cn' => '平台公告',
                    'tw' => '平台公告',
                ],
                'addAnnouncement' => [
                    'en' => 'Add Announcement',
                    'cn' => '新增平台公告',
                    'tw' => '新增平台公告',
                ],
                'modifyAnnouncement' => [
                    'en' => 'Modify Announcement',
                    'cn' => '平台公告修改',
                    'tw' => '平台公告修改',
                ],
                'downloadPage' => [
                    'en' => 'Download Page',
                    'cn' => '下载页',
                    'tw' => '下載頁',
                ],
                'rankingList' => [
                    'en' => 'Ranking List',
                    'cn' => '排行榜',
                    'tw' => '排行榜',
                ],
                'addRankList' => [
                    'en' => 'Add RankList Record',
                    'cn' => '新增排行榜数据',
                    'tw' => '新增排行榜數據',
                ],
                'modifyRankList' => [
                    'en' => 'Modify RankList Record',
                    'cn' => '排行榜数据修改',
                    'tw' => '排行榜數據修改',
                ],
                'deleteRankList' => [
                    'en' => 'Delete RankList Record',
                    'cn' => '排行榜数据删除',
                    'tw' => '排行榜數據刪除',
                ],
                'minerSetting' => [
                    'en' => 'Miner Setting',
                    'cn' => '矿机设置',
                    'tw' => '礦機設置',
                ],
                'addMiner' => [
                    'en' => 'Add Miner',
                    'cn' => '新增矿机',
                    'tw' => '新增礦機',
                ],
                'modifyMiner' => [
                    'en' => 'Modify Miner',
                    'cn' => '矿机修改',
                    'tw' => '礦機修改',
                ],
                'MinerDetail' => [
                    'en' => 'Miner Detail',
                    'cn' => '矿机详情',
                    'tw' => '礦機詳情',
                ],
                'groupOrderSetting' => [
                    'en' => 'Group Order Setting',
                    'cn' => '团购订单设置',
                    'tw' => '團購訂單設置',
                ],
                'addGroupOrder' => [
                    'en' => 'Add Group Order',
                    'cn' => '新增团购订单',
                    'tw' => '新增訂單團購',
                ],
                'modifyGroupOrder' => [
                    'en' => 'Modify Group Order',
                    'cn' => '团购订单修改',
                    'tw' => '團購訂單修改',
                ],
                'cancelGroupOrder' => [
                    'en' => 'Cancel Group Order',
                    'cn' => '取消团购订单',
                    'tw' => '取消團購訂單',
                ],
                'miningBlockSetting' => [
                    'en' => 'Mining Block Setting',
                    'cn' => '挖矿难度设置',
                    'tw' => '挖礦難度設置',
                ],
                'addMiningBlock' => [
                    'en' => 'Add Mining Block',
                    'cn' => '新增挖矿难度',
                    'tw' => '新增挖礦難度',
                ],
                'modifyMiningBlock' => [
                    'en' => 'Modify Mining Block',
                    'cn' => '挖矿难度修改',
                    'tw' => '挖礦難度修改',
                ],
                'miningBlockDetail' => [
                    'en' => 'Mining Block Detail',
                    'cn' => '挖矿难度详情',
                    'tw' => '挖礦難度詳情',
                ],
                'deleteMiningBlock' => [
                    'en' => 'Delete Mining Block',
                    'cn' => '挖矿难度删除',
                    'tw' => '挖礦難度刪除',
                ],
                'taskScheduling' => [
                    'en' => 'Task Scheduling',
                    'cn' => '定时任务',
                    'tw' => '定時任務',
                ],
                'addTask' => [
                    'en' => 'Add Task',
                    'cn' => '新增定时任务',
                    'tw' => '新增定時任務',
                ],
                'modifyTask' => [
                    'en' => 'Modify Task',
                    'cn' => '定时任务修改',
                    'tw' => '定時任務修改',
                ],
                'countryCodeMgmt' => [
                    'en' => 'Country Code Mgmt',
                    'cn' => '国家代码管理',
                    'tw' => '國家代碼管理',
                ],
                'addCountryCode' => [
                    'en' => 'Add Country Code',
                    'cn' => '新增国家代码',
                    'tw' => '新增國家代碼',
                ],
                'modifyCountryCode' => [
                    'en' => 'Modify Country Code',
                    'cn' => '国家代码修改',
                    'tw' => '國家代碼修改',
                ],
            ],
            'member' => [
                'title' => [
                    'en' => 'Member Mgmt',
                    'cn' => '会员管理',
                    'tw' => '會員管理',
                ],
                'memberList' => [
                    'en' => 'Member List',
                    'cn' => '会员列表',
                    'tw' => '會員列表',
                ],
                'addMember' => [
                    'en' => 'Add Member',
                    'cn' => '新增用户',
                    'tw' => '新增用戶',
                ],
                'modifyMember' => [
                    'en' => 'Modify Member',
                    'cn' => '个人资料修改',
                    'tw' => '個人資料修改',
                ],
                'memberDetail' => [
                    'en' => 'Member Detail',
                    'cn' => '会员详情',
                    'tw' => '會員詳情',
                ],
                'idInformation' => [
                    'en' => 'Id Information',
                    'cn' => '实名认证',
                    'tw' => '實名認證',
                ],
                'modifyIdInformation' => [
                    'en' => 'Modify Id Information',
                    'cn' => '实名认证修改',
                    'tw' => '實名認證修改',
                ],
                'idInformationDetail' => [
                    'en' => 'Id Information Detail',
                    'cn' => '实名认证详情',
                    'tw' => '實名認證詳情',
                ],
                'profitAnalysis' => [
                    'en' => 'Profit Analysis',
                    'cn' => '盈利分析',
                    'tw' => '盈利分析',
                ],
                'manualOperation' => [
                    'en' => 'Manual Operation',
                    'cn' => '人工操作',
                    'tw' => '人工操作',
                ],
                'addManualOperation' => [
                    'en' => 'Add Manual Operation',
                    'cn' => '新增人工操作',
                    'tw' => '新增人工操作',
                ],
            ],
            'agent' => [
                'title' => [
                    'en' => 'Agent Center',
                    'cn' => '代理中心',
                    'tw' => '代理中心',
                ],
                'agentDetail' => [
                    'en' => 'Agent Detail',
                    'cn' => '代理明细',
                    'tw' => '代理明細',
                ],
            ],
            'deposit' => [
                'title' => [
                    'en' => 'Deposit Mgmt',
                    'cn' => '充值管理',
                    'tw' => '充值管理',
                ],
                'depositList' => [
                    'en' => 'Deposit List',
                    'cn' => '充值明细',
                    'tw' => '充值明細',
                ],
                'modifyDeposit' => [
                    'en' => 'Modify Deposit',
                    'cn' => '充值修改',
                    'tw' => '充值修改',
                ],
                'depositDetail' => [
                    'en' => 'Deposit Detail',
                    'cn' => '充值详情',
                    'tw' => '充值詳情',
                ],
                'depositChannel' => [
                    'en' => 'Deposit Channel',
                    'cn' => '充值渠道',
                    'tw' => '充值渠道',
                ],
            ],
            'withdrawal' => [
                'title' => [
                    'en' => 'Withdrawal Mgmt',
                    'cn' => '提现管理',
                    'tw' => '提現管理',
                ],
                'withdrawalList' => [
                    'en' => 'Withdrawal List',
                    'cn' => '提现明细',
                    'tw' => '提現明細',
                ],
                'modifyWithdrawal' => [
                    'en' => 'Modify Withdrawal',
                    'cn' => '提现修改',
                    'tw' => '提現修改',
                ],
                'withdrawalDetail' => [
                    'en' => 'Modify Withdrawal',
                    'cn' => '提现详情',
                    'tw' => '提現詳情',
                ],
                'withdrawalChannel' => [
                    'en' => 'Withdrawal Channel',
                    'cn' => '提现渠道',
                    'tw' => '提現渠道',
                ],
                'addWithdrawal' => [
                    'en' => 'Add Withdrawal',
                    'cn' => '新增提现',
                    'tw' => '新增提現',
                ],
            ],
            'financialMgmt' => [
                'title' => [
                    'en' => 'Financial Mgmt',
                    'cn' => '财务管理',
                    'tw' => '財務管理',
                ],
                'purchaseList' => [
                    'en' => 'Purchase List',
                    'cn' => '销售订单',
                    'tw' => '銷售訂單',
                ],
                'rentList' => [
                    'en' => 'Rent List',
                    'cn' => '租赁订单',
                    'tw' => '租賃訂單',
                ],
                'addExperience' => [
                    'en' => 'Add Experience',
                    'cn' => '新增体验',
                    'tw' => '新增體驗',
                ],
                'groupList' => [
                    'en' => 'Group List',
                    'cn' => '团购订单',
                    'tw' => '團購訂單',
                ],
                'depreciationInfo' => [
                    'en' => 'Depreciation Info',
                    'cn' => '折旧信息',
                    'tw' => '折舊信息',
                ],
                'rebateInfo' => [
                    'en' => 'Rebate Info',
                    'cn' => '返佣信息',
                    'tw' => '返佣信息',
                ],
                'inviteRewardInfo' => [
                    'en' => 'Invite Reward Info',
                    'cn' => '邀请奖励信息',
                    'tw' => '邀請獎勵信息',
                ],
            ],
            'manager' => [
                'title' => [
                    'en' => 'Manager',
                    'cn' => '管理员',
                    'tw' => '管理員',
                ],
                'roleMgmt' => [
                    'en' => 'Role Mgmt',
                    'cn' => '角色管理',
                    'tw' => '角色管理',
                ],
                'addRole' => [
                    'en' => 'Add Role',
                    'cn' => '新增角色',
                    'tw' => '新增角色',
                ],
                'roleDetail' => [
                    'en' => 'Role Detail',
                    'cn' => '角色详情',
                    'tw' => '角色詳情',
                ],
                'modifyRole' => [
                    'en' => 'Modify Role',
                    'cn' => '角色修改',
                    'tw' => '角色修改',
                ],
                'modifyRoleMenus' => [
                    'en' => 'Modify Role Menus',
                    'cn' => '角色选单修改',
                    'tw' => '角色選單修改',
                ],
                'managerList' => [
                    'en' => 'Manager List',
                    'cn' => '管理员列表',
                    'tw' => '管理員列表',
                ],
                'addManager' => [
                    'en' => 'Add Manager',
                    'cn' => '新增管理员',
                    'tw' => '新增管理員',
                ],
                'managerDetail' => [
                    'en' => 'Manager Detail',
                    'cn' => '管理员详情',
                    'tw' => '管理員詳情',
                ],
                'modifyManager' => [
                    'en' => 'Modify Manager',
                    'cn' => '管理员修改',
                    'tw' => '管理員修改',
                ],
                'groupMgmt' => [
                    'en' => 'Group Mgmt',
                    'cn' => '组别管理',
                    'tw' => '組別管理',
                ],
                'operationLog' => [
                    'en' => 'Operation Log',
                    'cn' => '操作日志',
                    'tw' => '操作日誌',
                ],
                'operationDetail' => [
                    'en' => 'Operation Detail',
                    'cn' => '操作详情',
                    'tw' => '操作詳情',
                ],
            ],
            'report' => [
                'title' => [
                    'en' => 'Report',
                    'cn' => '报表',
                    'tw' => '報表',
                ],
                'intervalDailyReport' => [
                    'en' => 'Interval Daily Report',
                    'cn' => '数据区间日报表',
                    'tw' => '數據區間日報表',
                ],
            ],
        ];

        $result = [];
        foreach ($menuList as $title => $content) {
            foreach ($content as $menu => $lanList) {
                $result[$title][$menu] = $lanList[$lang];
            }
        }
        return $result;
    }
}
