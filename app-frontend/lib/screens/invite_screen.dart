import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../providers/member_provider.dart';
import '../themes/app_colors.dart';
import '../utils/utils.dart';
import '../widgets/base_card.dart';

class InviteScreen extends StatefulWidget {
  const InviteScreen({super.key});

  @override
  State<InviteScreen> createState() => _InviteScreenState();
}

class _InviteScreenState extends State<InviteScreen> {
  @override
  void initState() {
    context.read<MemberProvider>().fetchInviteData();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    final invite = context.watch<MemberProvider>().invite;
    final rebateData = invite?.rebate ?? [];
    final rewardData = invite?.reward ?? [];
    return SafeArea(
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            const SizedBox(height: 20),
            const Text(
              'Invite Your Friends To Miner',
              style: TextStyle(
                fontSize: 20,
                fontWeight: FontWeight.w600,
              ),
            ),
            const SizedBox(height: 16),
            const Text(
              'Copy the referral code\nto share with your friends',
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 20),
            BaseCard(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              child: Row(
                children: [
                  const Text('My Referral Code:'),
                  const SizedBox(width: 8),
                  Text(
                    invite?.inviteCode ?? '-',
                    style: TextStyle(
                      color: AppColors.primaryColor,
                    ),
                  ),
                  const Spacer(),
                  IconButton(
                    onPressed: () {
                      Clipboard.setData(
                        ClipboardData(text: invite?.inviteCode ?? ''),
                      );
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(
                          content: Text('Referral code copied'),
                        ),
                      );
                    },
                    icon: Icon(
                      Icons.copy,
                      color: AppColors.primaryColor,
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 8),
            Row(
              children: [
                const Text('Sales Rebate', style: TextStyle(fontSize: 16)),
                const Spacer(),
                IconButton(
                  onPressed: () {
                    context.push('/sales-rebates-help');
                  },
                  icon: Icon(
                    Icons.help_outline,
                    color: AppColors.primaryColor,
                  ),
                ),
              ],
            ),
            BaseCard(
              padding: const EdgeInsets.symmetric(
                horizontal: 16,
                vertical: 20,
              ),
              width: double.infinity,
              child: rebateData.isEmpty
                  ? const Center(
                      child: Text('No data'),
                    )
                  : Column(
                      children: [
                        const Row(
                          children: [
                            Expanded(
                              child: Text(
                                'Miner',
                                style: TextStyle(color: Colors.grey),
                              ),
                            ),
                            Expanded(
                              child: Text(
                                'Level',
                                textAlign: TextAlign.center,
                                style: TextStyle(color: Colors.grey),
                              ),
                            ),
                            Expanded(
                              child: Text(
                                'Sales Amount',
                                textAlign: TextAlign.center,
                                style: TextStyle(color: Colors.grey),
                              ),
                            ),
                            Expanded(
                              child: Text(
                                'Rebate',
                                textAlign: TextAlign.right,
                                style: TextStyle(color: Colors.grey),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 16),
                        for (var i = 0; i < rebateData.length; i++) ...[
                          Row(
                            children: [
                              Expanded(
                                child: Text(rebateData[i].minerName ?? '-'),
                              ),
                              Expanded(
                                child: Text(
                                  rebateData[i].level ?? '-',
                                  textAlign: TextAlign.center,
                                ),
                              ),
                              Expanded(
                                child: Text(
                                  formatNumber(rebateData[i].salesAmount),
                                  textAlign: TextAlign.center,
                                ),
                              ),
                              Expanded(
                                child: Text(
                                  formatNumber(rebateData[i].minerRebate),
                                  textAlign: TextAlign.right,
                                ),
                              ),
                            ],
                          ),
                          if (i != rebateData.length - 1)
                            const SizedBox(height: 16),
                        ],
                      ],
                    ),
            ),
            const SizedBox(height: 8),
            Row(
              children: [
                const Text('Referral Rewards', style: TextStyle(fontSize: 16)),
                const Spacer(),
                IconButton(
                  onPressed: () {
                    context.push('/referral-rewards-help');
                  },
                  icon: Icon(
                    Icons.help_outline,
                    color: AppColors.primaryColor,
                  ),
                ),
              ],
            ),
            BaseCard(
              padding: const EdgeInsets.symmetric(
                horizontal: 16,
                vertical: 20,
              ),
              width: double.infinity,
              child: rewardData.isEmpty
                  ? const Center(
                      child: Text('No data'),
                    )
                  : Column(
                      children: [
                        const Row(
                          children: [
                            Expanded(
                              child: Text(
                                'Miner',
                                style: TextStyle(color: Colors.grey),
                              ),
                            ),
                            Expanded(
                              child: Text(
                                'Quantity',
                                textAlign: TextAlign.center,
                                style: TextStyle(color: Colors.grey),
                              ),
                            ),
                            Expanded(
                              child: Text(
                                'Rewards',
                                textAlign: TextAlign.right,
                                style: TextStyle(color: Colors.grey),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 16),
                        for (var i = 0; i < rewardData.length; i++) ...[
                          Row(
                            children: [
                              Expanded(
                                child: Text(rewardData[i].minerName ?? '-'),
                              ),
                              Expanded(
                                child: Text(
                                  rewardData[i].minerQuantity ?? '-',
                                  textAlign: TextAlign.center,
                                ),
                              ),
                              Expanded(
                                child: Text(
                                  formatNumber(rewardData[i].minerReward),
                                  textAlign: TextAlign.right,
                                ),
                              ),
                            ],
                          ),
                          if (i != rewardData.length - 1)
                            const SizedBox(height: 16),
                        ],
                      ],
                    ),
            ),
          ],
        ),
      ),
    );
  }
}
