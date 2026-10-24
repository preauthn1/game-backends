import 'package:flutter/material.dart';
import 'package:miner/widgets/miner_list_view.dart';

import '../widgets/linear_gradient_background.dart';

class MinersScreen extends StatelessWidget {
  const MinersScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Miner'),
        ),
        body: const MinerListView(),
      ),
    );
  }
}
