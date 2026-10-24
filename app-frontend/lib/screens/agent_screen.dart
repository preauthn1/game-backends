import 'package:collection/collection.dart';
import 'package:flutter/material.dart';
import 'package:miner/providers/member_provider.dart';
import 'package:provider/provider.dart';

import '../widgets/base_card.dart';
import '../widgets/base_dropdown.dart';
import '../widgets/base_tab_bar.dart';
import '../widgets/linear_gradient_background.dart';

class AgentScreen extends StatefulWidget {
  const AgentScreen({super.key});

  @override
  State<AgentScreen> createState() => _AgentScreenState();
}

class _AgentScreenState extends State<AgentScreen> {
  late Future<void> _future;
  String? _teamSelectedDate;
  String? _teamSelectedLevel;
  String? _salesSelectedDate;
  String? _salesSelectedMiner;

  Widget _buildTeam() {
    final allList = context.read<MemberProvider>().agentCenter?.team ?? [];
    final dateList = allList
        .map((e) => e.createdAt?.substring(0, 10))
        .toSet()
        .sorted((a, b) => b!.compareTo(a!));
    final levelList =
        allList.map((e) => e.level).toSet().sorted((a, b) => a!.compareTo(b!));

    final filteredList = allList.where((element) {
      if (_teamSelectedDate != null &&
          element.createdAt?.substring(0, 10) != _teamSelectedDate) {
        return false;
      }
      if (_teamSelectedLevel != null && element.level != _teamSelectedLevel) {
        return false;
      }
      return true;
    }).toList();
    final agentCenter = context.read<MemberProvider>().agentCenter;

    return SingleChildScrollView(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          BaseCard(
            width: double.infinity,
            padding: const EdgeInsets.symmetric(vertical: 20, horizontal: 16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'Team',
                  style: TextStyle(
                    fontSize: 12,
                    color: Color(0xFFa3a2a4),
                  ),
                ),
                Text(
                  agentCenter?.teamCount ?? '-',
                  style: const TextStyle(fontSize: 32),
                ),
                const SizedBox(height: 16),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text(
                      'Sales Rebate',
                      style: TextStyle(color: Color(0xFFa3a2a4)),
                    ),
                    Text(agentCenter?.salesRebate ?? '-'),
                  ],
                ),
                const SizedBox(height: 10),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text(
                      'Referral Rewards',
                      style: TextStyle(color: Color(0xFFa3a2a4)),
                    ),
                    Text(agentCenter?.referralReward ?? '-'),
                  ],
                ),
              ],
            ),
          ),
          const SizedBox(height: 20),
          Row(
            children: [
              BaseDropdown.small(
                onChanged: (value) {
                  setState(() {
                    _teamSelectedDate = value as String?;
                  });
                },
                value: _teamSelectedDate,
                items: [
                  const DropdownMenuItem(
                    value: null,
                    child: Text('Date'),
                  ),
                  for (var item in dateList)
                    DropdownMenuItem(
                      value: item,
                      child: Text(item ?? '-'),
                    ),
                ],
              ),
              const SizedBox(width: 16),
              BaseDropdown.small(
                onChanged: (value) {
                  setState(() {
                    _teamSelectedLevel = value as String?;
                  });
                },
                value: _teamSelectedLevel,
                items: [
                  const DropdownMenuItem(
                    value: null,
                    child: Text('Level'),
                  ),
                  for (var item in levelList)
                    DropdownMenuItem(
                      value: item,
                      child: Text(item ?? '-'),
                    ),
                ],
              ),
            ],
          ),
          const SizedBox(height: 12),
          if (filteredList.isEmpty)
            const Center(
              child: Padding(
                padding: EdgeInsets.all(12.0),
                child: Text('No data'),
              ),
            ),
          if (filteredList.isNotEmpty)
            BaseCard(
              padding: const EdgeInsets.symmetric(
                horizontal: 16,
                vertical: 20,
              ),
              width: double.infinity,
              child: Column(
                children: [
                  const Row(
                    children: [
                      Expanded(
                        child: Text(
                          'Account',
                          style: TextStyle(color: Colors.grey),
                        ),
                      ),
                      Expanded(
                        child: Text(
                          'Lv.',
                          textAlign: TextAlign.center,
                          style: TextStyle(color: Colors.grey),
                        ),
                      ),
                      Expanded(
                        child: Text(
                          'Sign up date',
                          textAlign: TextAlign.right,
                          style: TextStyle(color: Colors.grey),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 16),
                  for (var i = 0; i < filteredList.length; i++) ...[
                    Row(
                      children: [
                        Expanded(
                          child: Text(filteredList[i].account ?? '-'),
                        ),
                        Expanded(
                          child: Text(
                            filteredList[i].level ?? '-',
                            textAlign: TextAlign.center,
                          ),
                        ),
                        Expanded(
                          child: Text(
                            filteredList[i].createdAt ?? '-',
                            textAlign: TextAlign.right,
                          ),
                        ),
                      ],
                    ),
                    if (i != filteredList.length - 1)
                      const SizedBox(height: 16),
                  ],
                ],
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildSales() {
    final allList = context.read<MemberProvider>().agentCenter?.sales ?? [];
    final dateList = allList
        .map((e) => e.createdAt?.substring(0, 10))
        .toSet()
        .sorted((a, b) => b!.compareTo(a!));
    final minerList = allList
        .map((e) => e.minerName)
        .toSet()
        .sorted((a, b) => a!.compareTo(b!));

    final filteredList = allList.where((element) {
      if (_salesSelectedDate != null &&
          element.createdAt?.substring(0, 10) != _salesSelectedDate) {
        return false;
      }
      if (_salesSelectedMiner != null &&
          element.minerName != _salesSelectedMiner) {
        return false;
      }
      return true;
    }).toList();

    return SingleChildScrollView(
      padding: const EdgeInsets.all(20),
      child: Column(
        children: [
          Row(
            children: [
              BaseDropdown.small(
                onChanged: (value) {
                  setState(() {
                    _salesSelectedDate = value as String?;
                  });
                },
                value: _salesSelectedDate,
                items: [
                  const DropdownMenuItem(
                    value: null,
                    child: Text('Date'),
                  ),
                  for (var item in dateList)
                    DropdownMenuItem(
                      value: item,
                      child: Text(item ?? '-'),
                    ),
                ],
              ),
              const SizedBox(width: 16),
              BaseDropdown.small(
                onChanged: (value) {
                  setState(() {
                    _salesSelectedMiner = value as String?;
                  });
                },
                value: _salesSelectedMiner,
                items: [
                  const DropdownMenuItem(
                    value: null,
                    child: Text('Miner'),
                  ),
                  for (var item in minerList)
                    DropdownMenuItem(
                      value: item,
                      child: Text(item ?? '-'),
                    ),
                ],
              ),
            ],
          ),
          const SizedBox(height: 12),
          if (filteredList.isEmpty)
            const Center(
              child: Padding(
                padding: EdgeInsets.all(12.0),
                child: Text('No data'),
              ),
            ),
          if (filteredList.isNotEmpty)
            BaseCard(
              padding: const EdgeInsets.symmetric(
                horizontal: 16,
                vertical: 20,
              ),
              width: double.infinity,
              child: Column(
                children: [
                  const Row(
                    children: [
                      Expanded(
                        child: Text(
                          'Account',
                          style: TextStyle(color: Colors.grey),
                        ),
                      ),
                      Expanded(
                        child: Text(
                          'Miner',
                          textAlign: TextAlign.center,
                          style: TextStyle(color: Colors.grey),
                        ),
                      ),
                      Expanded(
                        child: Text(
                          'QTY',
                          textAlign: TextAlign.center,
                          style: TextStyle(color: Colors.grey),
                        ),
                      ),
                      Expanded(
                        child: Text(
                          'Time',
                          textAlign: TextAlign.right,
                          style: TextStyle(color: Colors.grey),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 16),
                  for (var i = 0; i < filteredList.length; i++) ...[
                    Row(
                      children: [
                        Expanded(
                          child: Text(filteredList[i].username ?? '-'),
                        ),
                        Expanded(
                          child: Text(
                            filteredList[i].minerName ?? '-',
                            textAlign: TextAlign.center,
                          ),
                        ),
                        Expanded(
                          child: Text(
                            filteredList[i].qty ?? '-',
                            textAlign: TextAlign.center,
                          ),
                        ),
                        Expanded(
                          child: Text(
                            filteredList[i].createdAt ?? '-',
                            textAlign: TextAlign.right,
                          ),
                        ),
                      ],
                    ),
                    if (i != filteredList.length - 1)
                      const SizedBox(height: 16),
                  ],
                ],
              ),
            ),
        ],
      ),
    );
  }

  @override
  void initState() {
    _future = context.read<MemberProvider>().fetchAgentCenter();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: FutureBuilder(
        future: _future,
        builder: (context, snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return Scaffold(
              appBar: AppBar(
                title: const Text('Agent Center'),
              ),
              body: const Center(
                child: CircularProgressIndicator(),
              ),
            );
          }

          if (snapshot.hasError) {
            return Scaffold(
              appBar: AppBar(
                title: const Text('Agent Center'),
              ),
              body: const Center(
                child: Text('Error'),
              ),
            );
          }

          return DefaultTabController(
            length: 2,
            child: Scaffold(
              appBar: AppBar(
                title: const Text('Agent Center'),
                bottom: const BaseTabBar(
                  tabs: [
                    Tab(text: 'Team Overview'),
                    Tab(text: 'Sales Overview'),
                  ],
                ),
              ),
              body: TabBarView(
                children: [
                  _buildTeam(),
                  _buildSales(),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
