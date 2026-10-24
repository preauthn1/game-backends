import 'package:flutter/material.dart';
import 'package:miner/utils/utils.dart';
import 'package:provider/provider.dart';

import '../models/my_miner_detail.dart';
import '../providers/miner_provider.dart';
import '../widgets/base_button.dart';
import '../widgets/base_dialog.dart';
import '../widgets/base_text_field.dart';
import '../widgets/linear_gradient_background.dart';
import '../widgets/scrollable_expanded_column.dart';

class MyMinerBalanceScreen extends StatefulWidget {
  final String mmId;
  const MyMinerBalanceScreen({super.key, required this.mmId});

  @override
  State<MyMinerBalanceScreen> createState() => _MyMinerBalanceScreenState();
}

class _MyMinerBalanceScreenState extends State<MyMinerBalanceScreen> {
  final TextEditingController _controller = TextEditingController();
  MyMinerDetail? _myMinerDetail;
  bool _isSelling = false;

  @override
  void initState() {
    if (widget.mmId == context.read<MinerProvider>().myMinerDetail?.mmId) {
      _myMinerDetail = context.read<MinerProvider>().myMinerDetail;
      _future();
    }
    super.initState();
  }

  Future<void> _future() async {
    final balance =
        await context.read<MinerProvider>().fetchMyMinerBalance(widget.mmId);
    if (!mounted) return;
    _controller.text = formatNumber(balance);
  }

  Future<void> _sellBackToPlatform() async {
    setState(() {
      _isSelling = true;
    });
    try {
      if (_myMinerDetail?.type == "1") {
        await context.read<MinerProvider>().sellBackMiner(widget.mmId);
      } else if (_myMinerDetail?.type == "2") {
        await context.read<MinerProvider>().sellBackGroupMiner(widget.mmId);
      } else {
        await showDialog(
          context: context,
          builder: (context) {
            return BaseDialog(
              titleText: "Error",
              message: 'The miner type is incorrect',
              onConfirm: () {
                Navigator.of(context).pop();
              },
            );
          },
        );
        return;
      }
      if (!mounted) return;
      await context.read<MinerProvider>().fetchMyPurchaseMiners();
      if (!mounted) return;
      await showDialog(
        context: context,
        builder: (context) {
          return BaseDialog(
            titleText: "Sold successfully",
            message:
                'The miner has been successfully sold back to the platform',
            onConfirm: () {
              Navigator.of(context).pop();
            },
          );
        },
      );
      if (!mounted) return;
      Navigator.of(context).pop();
      Navigator.of(context).pop();
    } finally {
      setState(() {
        _isSelling = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('My Miner Balance'),
        ),
        body: _myMinerDetail == null
            ? const Center(child: Text('Not found'))
            : ScrollableExpandedColumn(
                children: [
                  BaseTextField(
                    controller: _controller,
                    labelText: 'Balance',
                    readOnly: true,
                  ),
                  const SizedBox(height: 20),
                  BaseButton(
                    onPressed: _sellBackToPlatform,
                    loading: _isSelling,
                    text: 'Sell ​​back to the platform',
                  ),
                ],
              ),
      ),
    );
  }
}
