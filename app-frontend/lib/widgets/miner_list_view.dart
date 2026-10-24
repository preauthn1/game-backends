import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../providers/miner_provider.dart';
import 'base_tab_bar.dart';
import 'miner_list.dart';

class MinerListView extends StatefulWidget {
  const MinerListView({super.key});

  @override
  State<MinerListView> createState() => _MinerListViewState();
}

class _MinerListViewState extends State<MinerListView> {
  Widget _buildPurchase(BuildContext context) {
    const space = 10.0;
    return SingleChildScrollView(
      padding: const EdgeInsets.all(space / 2),
      child: SafeArea(
        child: MinerList.purchase(
          space: space,
          purchaseMiners: context.watch<MinerProvider>().purchaseMiners,
          onTapPurchase: (miner) => context.push(
            '/miners/purchase/${miner.id}/detail',
          ),
        ),
      ),
    );
  }

  Widget _buildRent(BuildContext context) {
    const space = 10.0;
    return SingleChildScrollView(
      padding: const EdgeInsets.all(space / 2),
      child: MinerList.rent(
        space: space,
        rentMiners: context.watch<MinerProvider>().rentMiners,
        onTapRent: (miner) => context.push(
          '/miners/rent/${miner.id}/list',
        ),
      ),
    );
  }

  @override
  void initState() {
    context.read<MinerProvider>().fetchPurchaseMiners();
    context.read<MinerProvider>().fetchRentMiners();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return DefaultTabController(
      length: 2,
      child: Column(
        children: [
          const BaseTabBar(
            tabs: [
              Tab(text: 'Purchase'),
              Tab(text: 'Rent'),
            ],
          ),
          Expanded(
            child: TabBarView(
              children: [
                _buildPurchase(context),
                _buildRent(context),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
