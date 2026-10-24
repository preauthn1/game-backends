import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/purchase_miner.dart';
import '../providers/miner_provider.dart';
import '../themes/app_colors.dart';
import '../utils/utils.dart';
import '../widgets/base_button.dart';
import '../widgets/base_card.dart';
import '../widgets/base_dialog.dart';
import '../widgets/linear_gradient_background.dart';
import '../widgets/quantity_selector.dart';

class MinerPurchaseCheckoutScreen extends StatefulWidget {
  final String id;
  const MinerPurchaseCheckoutScreen({super.key, required this.id});

  @override
  State<MinerPurchaseCheckoutScreen> createState() =>
      _MinerPurchaseCheckoutScreenState();
}

class _MinerPurchaseCheckoutScreenState
    extends State<MinerPurchaseCheckoutScreen> {
  int _quantity = 1;
  PurchaseMiner? _miner;
  bool _isPurchasing = false;

  double get _totalAmount {
    if (_miner == null) return 0;
    final price = double.tryParse(_miner!.purchasePrice ?? '0') ?? 0;
    return price * _quantity;
  }

  String? get _minerId => _miner?.id;

  @override
  void initState() {
    _miner = context.read<MinerProvider>().getPurchaseMinerById(widget.id);
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    int? max = _miner?.count != null ? int.tryParse(_miner!.count!) : null;
    return LinearGradientBackground(
      child: SafeArea(
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
                            formatNumber(_totalAmount),
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
                        padding: const EdgeInsets.symmetric(
                          horizontal: 28,
                          vertical: 14,
                        ),
                        loading: _isPurchasing,
                        onPressed: _minerId == null
                            ? null
                            : () async {
                                if (_isPurchasing) return;
                                setState(() {
                                  _isPurchasing = true;
                                });
                                try {
                                  await context
                                      .read<MinerProvider>()
                                      .purchaseMiner(
                                        minerId: _minerId!,
                                        quantity: _quantity,
                                      );
                                  if (!context.mounted) return;
                                  await context
                                      .read<MinerProvider>()
                                      .fetchPurchaseMiners();
                                  if (!context.mounted) return;
                                  await context
                                      .read<MinerProvider>()
                                      .fetchRentMiners();
                                  if (!context.mounted) return;
                                  await showDialog(
                                    context: context,
                                    builder: (context) => BaseDialog(
                                      message: 'Purchase successful',
                                      onConfirm: () =>
                                          Navigator.of(context).pop(),
                                    ),
                                  );
                                  if (!context.mounted) return;
                                  Navigator.of(context).pop();
                                } finally {
                                  setState(() {
                                    _isPurchasing = false;
                                  });
                                }
                              },
                        text: 'BUY',
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
                          QuantitySelector(
                            quantity: _quantity,
                            min: 1,
                            max: max,
                            onChanged: (value) {
                              setState(() {
                                _quantity = value;
                              });
                            },
                          ),
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
      ),
    );
  }
}
