import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../providers/miner_provider.dart';
import '../widgets/linear_gradient_background.dart';
import '../widgets/miner_list.dart';

class MinersGroupScreen extends StatefulWidget {
  const MinersGroupScreen({super.key});

  @override
  State<MinersGroupScreen> createState() => _MinersGroupScreenState();
}

class _MinersGroupScreenState extends State<MinersGroupScreen> {
  @override
  void initState() {
    context.read<MinerProvider>().fetchGroupMiners();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    const space = 10.0;
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Group Order'),
        ),
        body: SingleChildScrollView(
          padding: const EdgeInsets.all(space / 2),
          child: SafeArea(
            child: MinerList.purchase(
              purchaseMiners: context.watch<MinerProvider>().groupMiners,
              space: space,
              onTapPurchase: (miner) => context.push(
                '/miners/group/${miner.id}/detail',
              ),
            ),
          ),
        ),
      ),
    );
  }
}
