import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/miner_group_order.dart';
import '../models/purchase_miner.dart';
import '../providers/miner_provider.dart';
import '../themes/app_colors.dart';
import '../utils/utils.dart';
import '../widgets/base_button.dart';
import '../widgets/base_card.dart';
import '../widgets/base_dialog.dart';
import '../widgets/linear_gradient_background.dart';

class MinerGroupCheckoutScreen extends StatefulWidget {
  final String minerId;
  final String? mmId;
  const MinerGroupCheckoutScreen({
    super.key,
    required this.minerId,
    this.mmId,
  });

  @override
  State<MinerGroupCheckoutScreen> createState() =>
      _MinerGroupCheckoutScreenState();
}

class _MinerGroupCheckoutScreenState extends State<MinerGroupCheckoutScreen> {
  PurchaseMiner? _miner;
  MinerGroupOrder? _groupOrder;
  bool _isSubmitting = false;

  int? get _type {
    if (widget.mmId != null && _groupOrder?.type == "0") return 0;
    if (widget.mmId != null && _groupOrder?.type == "1") return 1;
    if (widget.mmId == null) return 2;
    return null;
  }

  String get _btnText {
    if (_type == 0) return 'CANCEL';
    if (_type == 1) return 'BUY';
    if (_type == 2) return 'CREATE';
    return '-';
  }

  Future<void> _submit() async {
    if (_isSubmitting) return Future.value();
    setState(() {
      _isSubmitting = true;
    });
    try {
      // await checkIdInformation(context);
      if (_type == 0) {
        if (widget.mmId == null) return;
        await context.read<MinerProvider>().cancelGroupMiner(widget.mmId!);
      } else if (_type == 1) {
        if (widget.mmId == null) return;
        await context.read<MinerProvider>().purchaseGroupMiner(widget.mmId!);
      } else if (_type == 2) {
        await context.read<MinerProvider>().createGroupMiner(widget.minerId);
      } else {
        return;
      }
      if (!mounted) return;
      await showDialog(
        context: context,
        builder: (context) => BaseDialog(
          message: 'Success',
          onConfirm: () => Navigator.of(context).pop(),
        ),
      );
      if (!mounted) return;
      Navigator.of(context).pop();
    } finally {
      setState(() {
        _isSubmitting = false;
      });
    }
  }

  @override
  void initState() {
    _miner = context.read<MinerProvider>().getGroupMinerById(widget.minerId);
    if (widget.mmId != null) {
      _groupOrder =
          context.read<MinerProvider>().getGroupOrderById(widget.mmId!);
    }

    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Purchase'),
        ),
        bottomNavigationBar: Container(
          height: 80,
          padding: const EdgeInsets.all(16),
          decoration: const BoxDecoration(
            border: Border(
              top: BorderSide(
                color: Color(0xff252526),
              ),
            ),
          ),
          child: _miner == null
              ? const Center(child: Text('Miner not found'))
              : Row(
                  crossAxisAlignment: CrossAxisAlignment.center,
                  children: [
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Text('Total amount'),
                        Text(
                          formatNumber(_miner?.purchasePrice),
                          style: TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.w600,
                            color: AppColors.primaryColor,
                          ),
                        ),
                      ],
                    ),
                    const Spacer(),
                    BaseButton.small(
                      tag: _type == 0 ? 'cancel' : null,
                      padding: const EdgeInsets.symmetric(
                        horizontal: 28,
                        vertical: 14,
                      ),
                      loading: _isSubmitting,
                      onPressed: _submit,
                      text: _btnText,
                    ),
                  ],
                ),
        ),
        body: SingleChildScrollView(
          padding: const EdgeInsets.all(20.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              BaseCard(
                padding: const EdgeInsets.all(16),
                child: Row(
                  children: [
                    BaseCard(
                      width: 88,
                      height: 88,
                      child: Image.network(
                        _miner?.image ?? '',
                        fit: BoxFit.cover,
                      ),
                    ),
                    const SizedBox(width: 10),
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(_miner?.name ?? ''),
                        const SizedBox(height: 8),
                        Text(
                          '${formatNumber(_miner?.purchasePrice)} USDT',
                          style: TextStyle(
                            fontWeight: FontWeight.w600,
                            color: AppColors.accentColor,
                          ),
                        ),
                        const SizedBox(height: 16),
                        const BaseCard(
                          padding: EdgeInsets.symmetric(
                            horizontal: 18,
                            vertical: 4,
                          ),
                          child: Text('x1'),
                        )
                      ],
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 20),
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Icon(
                    Icons.help_outline,
                    color: AppColors.primaryColor,
                  ),
                  const SizedBox(width: 8),
                  const Expanded(
                    child: Text(
                      'Please carefully review your order before confirming your purchase. All sales are considered final. We do not offer refunds or exchanges for any products or services sold through xxx.',
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}
