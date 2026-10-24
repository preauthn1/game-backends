import '../platform/web_js.dart'
    if (example.invalid) '../platform/mobile_stub.dart';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../providers/app_provider.dart';
import '../providers/auth_provider.dart';
import '../providers/member_provider.dart';
import '../providers/wallet_provider.dart';
import '../themes/app_colors.dart';
import '../utils/utils.dart';
import '../widgets/balance_text.dart';
import '../widgets/base_button.dart';
import '../widgets/base_card.dart';
import '../widgets/base_dialog.dart';
import '../widgets/version_text.dart';

class MyScreen extends StatefulWidget {
  const MyScreen({super.key});

  @override
  State<MyScreen> createState() => _MyScreenState();
}

class _MyScreenState extends State<MyScreen> {
  bool _loadingCS = false;
  final navData = [
    {
      'icon': 'assets/images/my/profile.svg',
      'text': 'Profile',
      'path': '/profile',
    },
    {
      'icon': 'assets/images/my/fund_records.svg',
      'text': 'Fund Records',
      'path': '/record/fund',
    },
    {
      'icon': 'assets/images/my/top_up_details.svg',
      'text': 'Deposit Details',
      'path': '/record/top-up',
    },
    {
      'icon': 'assets/images/my/withdrawal_details.svg',
      'text': 'Withdrawal Details',
      'path': '/record/withdrawal',
    },
    {
      'icon': 'assets/images/my/depreciation.svg',
      'text': 'Depreciation',
      'path': '/trial-depreciation',
    },
    {
      'icon': 'assets/images/my/sales_rebate.svg',
      'text': 'Sales Rebate',
      'path': '/sales-rebate',
    },
    {
      'icon': 'assets/images/my/referral_rewards.svg',
      'text': 'Referral Rewards',
      'path': '/referral-reward',
    },
    {
      'icon': 'assets/images/my/faq.svg',
      'text': 'FAQ',
      'path': '/faq',
    },
  ];

  Future<void> _clickCS() async {
    if (_loadingCS) return;
    setState(() {
      _loadingCS = true;
    });
    try {
      final provider = context.read<AuthProvider>();
      if (provider.customerService == null) {
        await provider.fetchCustomerService();
        chatLogin(data: provider.customerService!);
        if (!mounted) return;
      }

      if (provider.customerService == null) {
        await showDialog(
          context: context,
          builder: (context) {
            return BaseDialog(
              titleText: "Error",
              message: 'Cannot get customer service information.',
              onConfirm: () {
                Navigator.of(context).pop();
              },
            );
          },
        );
        return;
      }

      chatOpen(
        context: context,
        data: provider.customerService!,
      );
    } finally {
      setState(() {
        _loadingCS = false;
      });
    }
  }

  void logout() async {
    clearProvider(context);
    context.go('/login');
  }

  @override
  void initState() {
    context.read<WalletProvider>().fetchBalance();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          children: [
            BaseCard(
              width: double.infinity,
              child: Column(
                children: [
                  const Padding(
                    padding: EdgeInsets.all(20),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Balance',
                          style: TextStyle(fontSize: 12),
                        ),
                        BalanceText(),
                      ],
                    ),
                  ),
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.symmetric(vertical: 20),
                    decoration: BoxDecoration(
                      borderRadius: BorderRadius.circular(8),
                      color: AppColors.primaryColor,
                    ),
                    child: Row(
                      children: [
                        Expanded(
                          child: Column(
                            children: [
                              const Text(
                                'Total Proceeds',
                                style: TextStyle(
                                  fontSize: 12,
                                  color: Colors.black,
                                ),
                              ),
                              Text(
                                formatNumber(context
                                        .watch<MemberProvider>()
                                        .member
                                        ?.totalProceeds ??
                                    0),
                                style: const TextStyle(
                                  fontSize: 20,
                                  fontWeight: FontWeight.w600,
                                  color: Colors.black,
                                ),
                              ),
                            ],
                          ),
                        ),
                        Container(
                          width: 1,
                          height: 40,
                          color: Colors.black,
                        ),
                        Expanded(
                          child: Column(
                            children: [
                              const Text(
                                'Today\'s earnings',
                                style: TextStyle(
                                  fontSize: 12,
                                  color: Colors.black,
                                ),
                              ),
                              Text(
                                formatNumber(context
                                        .watch<MemberProvider>()
                                        .member
                                        ?.todayEarning ??
                                    0),
                                style: const TextStyle(
                                  fontSize: 20,
                                  fontWeight: FontWeight.w600,
                                  color: Colors.black,
                                ),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 20),
            Row(
              children: [
                Expanded(
                  child: BaseButton(
                    tag: 'my-top-up',
                    onPressed: () {
                      context.push('/top-up');
                    },
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        SvgPicture.asset(
                          'assets/images/my/top_up.svg',
                          height: 16,
                        ),
                        const SizedBox(width: 4),
                        Text(
                          'Deposit',
                          style: TextStyle(
                            fontWeight: FontWeight.w600,
                            color: AppColors.primaryColor,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(width: 15),
                Expanded(
                  child: BaseButton(
                    tag: 'my-withdrawal',
                    onPressed: () async {
                      final template = context.read<AppProvider>().template;
                      if (template?.withdrawalRequiresVerifiedId ?? true) {
                        await checkIdInformation(context);
                        if (!context.mounted) return;
                      }
                      context.push('/withdrawal');
                    },
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        SvgPicture.asset(
                          'assets/images/my/withdrawal.svg',
                          height: 16,
                        ),
                        const SizedBox(width: 4),
                        Text(
                          'Withdraw',
                          style: TextStyle(
                            fontWeight: FontWeight.w600,
                            color: AppColors.primaryColor,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 20),
            BaseCard(
              padding: const EdgeInsets.symmetric(
                vertical: 18,
              ),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  {
                    'icon': 'assets/images/my/promotion.svg',
                    'text': 'Promotion',
                    'path': '/promotion',
                  },
                  {
                    'icon': 'assets/images/my/agent.svg',
                    'text': 'Agent',
                    'path': '/agent',
                  },
                  {
                    'icon': 'assets/images/my/group_order.svg',
                    'text': 'Group Order',
                    'path': '/miners/group',
                  },
                  {
                    'icon': 'assets/images/my/cs.svg',
                    'text': 'CS',
                    'type': 'cs',
                  },
                ].map((item) {
                  return Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.center,
                      children: [
                        Material(
                          color: const Color(0xFF3b363f),
                          borderRadius: BorderRadius.circular(50),
                          child: SizedBox(
                            width: 50,
                            height: 50,
                            child: InkWell(
                              borderRadius: BorderRadius.circular(50),
                              onTap: () {
                                if (item['path'] != null) {
                                  context.push(item['path']!);
                                } else if (item['type'] == 'cs') {
                                  _clickCS();
                                }
                              },
                              child: Padding(
                                padding: const EdgeInsets.all(8.0),
                                child: SvgPicture.asset(
                                  item['icon']!,
                                ),
                              ),
                            ),
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          item['text']!,
                          textAlign: TextAlign.center,
                          style: const TextStyle(fontSize: 12),
                        ),
                      ],
                    ),
                  );
                }).toList(),
              ),
            ),
            const SizedBox(height: 20),
            BaseCard(
              child: Column(
                children: navData.asMap().entries.map(
                  (entry) {
                    int index = entry.key;
                    Map<String, String> item = entry.value;
                    BorderRadius? borderRadius;
                    const circleRadius = 10.0;
                    if (index == 0) {
                      borderRadius = const BorderRadius.only(
                        topLeft: Radius.circular(circleRadius),
                        topRight: Radius.circular(circleRadius),
                      );
                    } else if (index == navData.length - 1) {
                      borderRadius = const BorderRadius.only(
                        bottomLeft: Radius.circular(circleRadius),
                        bottomRight: Radius.circular(circleRadius),
                      );
                    }

                    return ListTile(
                      shape: RoundedRectangleBorder(
                        borderRadius: borderRadius ?? BorderRadius.zero,
                      ),
                      leading: SizedBox(
                        width: 24,
                        height: 24,
                        child: SvgPicture.asset(item['icon']!),
                      ),
                      title: Text(item['text']!),
                      trailing: const Icon(Icons.chevron_right),
                      onTap: () {
                        context.push(item['path']!);
                      },
                    );
                  },
                ).toList(),
              ),
            ),
            const SizedBox(height: 20),
            BaseButton(
              tag: 'logout',
              text: 'Logout',
              onPressed: logout,
            ),
            const SizedBox(height: 10),
            if (!kIsWeb)
              const Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: [VersionText()],
              ),
          ],
        ),
      ),
    );
  }
}
