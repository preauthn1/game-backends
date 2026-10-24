import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../models/rent_miner.dart';
import '../providers/miner_provider.dart';
import '../themes/app_colors.dart';
import '../utils/utils.dart';
import '../widgets/base_button.dart';
import '../widgets/base_dialog.dart';
import '../widgets/linear_gradient_background.dart';

class MinerRentListScreen extends StatefulWidget {
  final int minerId;
  const MinerRentListScreen({super.key, required this.minerId});

  @override
  State<MinerRentListScreen> createState() => _MinerRentListScreenState();
}

class _MinerRentListScreenState extends State<MinerRentListScreen> {
  late Future<void> _future;
  final List<RentMiner> _selectedMiners = [];
  bool _isSubmitting = false;

  num get _totalAmount => _selectedMiners.fold(
        0,
        (prev, rent) => prev + (num.tryParse(rent.rentPrice ?? '0') ?? 0),
      );

  Future<void> _submit() async {
    setState(() {
      _isSubmitting = true;
    });
    try {
      // await checkIdInformation(context);
      if (!mounted) return;
      final ids = _selectedMiners
          .where((e) => e.mmId != null)
          .map((e) => e.mmId!)
          .toList();
      await context.read<MinerProvider>().doRentMiners(ids);
      if (!mounted) return;
      await showDialog(
        context: context,
        builder: (context) {
          return BaseDialog(
            titleText: 'Success',
            message: 'You have completed your rental.',
            onConfirm: () {
              Navigator.of(context).pop();
            },
          );
        },
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
    _future = context.read<MinerProvider>().fetchRentList(widget.minerId);
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    final list = context.watch<MinerProvider>().rentList;
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Rent'),
        ),
        bottomNavigationBar: list.isEmpty
            ? null
            : SafeArea(
                child: Container(
                  height: 80,
                  padding: const EdgeInsets.all(16),
                  decoration: const BoxDecoration(
                    border: Border(
                      top: BorderSide(
                        color: Color(0xff252526),
                      ),
                    ),
                  ),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: [
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          const Text('Total amount'),
                          Text(
                            '$_totalAmount',
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
                        loading: _isSubmitting,
                        padding: const EdgeInsets.symmetric(
                          horizontal: 28,
                          vertical: 14,
                        ),
                        text: 'Rent',
                        onPressed: _selectedMiners.isEmpty ? null : _submit,
                      ),
                    ],
                  ),
                ),
              ),
        body: FutureBuilder(
          future: _future,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(
                child: CircularProgressIndicator(),
              );
            }
            if (snapshot.hasError) {
              return Center(
                child: Text('Error: ${snapshot.error}'),
              );
            }

            if (list.isEmpty) {
              return const Center(
                child: Text('No data'),
              );
            }

            return ListView(
              padding: const EdgeInsets.all(20),
              children: list.map(
                (rent) {
                  return Row(
                    children: [
                      Checkbox(
                        value: _selectedMiners.contains(rent),
                        onChanged: (value) {
                          setState(() {
                            if (value == true) {
                              _selectedMiners.add(rent);
                            } else {
                              _selectedMiners.remove(rent);
                            }
                          });
                        },
                      ),
                      Expanded(
                        child: InkWell(
                          borderRadius: BorderRadius.circular(8),
                          onTap: () {
                            context.push(
                              '/miners/rent/${widget.minerId}/${rent.mmId}',
                            );
                          },
                          child: Padding(
                            padding: const EdgeInsets.all(8),
                            child: Row(
                              children: [
                                Text(rent.name ?? '-'),
                                const Text(' / '),
                                Text('Cycle ${rent.cycle ?? '-'}'),
                                const Spacer(),
                                Text(
                                  '${formatNumber(rent.rentPrice)} USDT',
                                  style: TextStyle(
                                    color: AppColors.primaryColor,
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),
                    ],
                  );
                },
              ).toList(),
            );
          },
        ),
      ),
    );
  }
}
