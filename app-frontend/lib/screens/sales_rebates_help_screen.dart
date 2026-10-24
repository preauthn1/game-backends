import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/miner_provider.dart';
import '../widgets/base_card.dart';
import '../widgets/linear_gradient_background.dart';

class SalesRebatesHelpScreen extends StatelessWidget {
  const SalesRebatesHelpScreen({super.key});

  String toPercent(String value) {
    try {
      double number = double.parse(value);
      return '${(number * 100).toStringAsFixed(2)}%';
    } catch (e) {
      return '-';
    }
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Sales Rebates'),
        ),
        body: FutureBuilder(
          future: context.read<MinerProvider>().fetchRebateList(),
          builder: (context, snapshot) {
            if (snapshot.connectionState != ConnectionState.done) {
              return const Center(
                child: CircularProgressIndicator(),
              );
            }

            if (snapshot.hasError) {
              return const Center(
                child: Text('Error'),
              );
            }

            final list = context.read<MinerProvider>().rebates;
            return SingleChildScrollView(
              padding: const EdgeInsets.symmetric(
                horizontal: 16,
                vertical: 20,
              ),
              child: Column(
                children: [
                  const Text(
                    'According to the mining purchased by the subordinates you invite, you will receive different percentages of sales rebates.',
                  ),
                  const SizedBox(height: 20),
                  SingleChildScrollView(
                    scrollDirection: Axis.horizontal,
                    child: BaseCard(
                      child: DataTable(
                        columns: const [
                          DataColumn(label: Text('Mining')),
                          DataColumn(label: Text('First Level Rebate')),
                          DataColumn(label: Text('Second Level Rebate')),
                        ],
                        rows: [
                          ...list.map(
                            (item) => DataRow(
                              cells: [
                                DataCell(Text(item.minerName ?? '-')),
                                DataCell(
                                  Text(toPercent(item.percentage2 ?? '')),
                                ),
                                DataCell(
                                  Text(toPercent(item.percentage3 ?? '')),
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ),
                  )
                ],
              ),
            );
          },
        ),
      ),
    );
  }
}
