import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../providers/miner_provider.dart';
import '../widgets/base_button.dart';
import '../widgets/base_card.dart';
import '../widgets/linear_gradient_background.dart';
import '../widgets/scrollable_expanded_column.dart';

class MinerGroupListScreen extends StatefulWidget {
  final int minerId;
  const MinerGroupListScreen({super.key, required this.minerId});

  @override
  State<MinerGroupListScreen> createState() => _MinerGroupListScreenState();
}

class _MinerGroupListScreenState extends State<MinerGroupListScreen> {
  late Future<void> _future;

  @override
  void initState() {
    _future = _refresh();
    super.initState();
  }

  Future<void> _refresh() async {
    await context.read<MinerProvider>().fetchGroupOrders(widget.minerId);
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Group Order'),
        ),
        body: FutureBuilder(
          future: _future,
          builder: (context, snapshot) {
            if (snapshot.connectionState != ConnectionState.done) {
              return const Center(
                child: CircularProgressIndicator(),
              );
            }

            if (snapshot.hasError) {
              return Center(
                child: Text(
                  snapshot.error.toString(),
                ),
              );
            }
            final list = context.watch<MinerProvider>().groupOrders;

            return ScrollableExpandedColumn(
              children: [
                list.isEmpty
                    ? const Text('No group orders')
                    : Column(
                        children: list.map(
                          (e) {
                            return BaseCard(
                              padding: const EdgeInsets.symmetric(
                                vertical: 12,
                                horizontal: 16,
                              ),
                              margin: const EdgeInsets.only(bottom: 10),
                              width: double.infinity,
                              child: Row(
                                mainAxisAlignment:
                                    MainAxisAlignment.spaceBetween,
                                children: [
                                  Text(e.username ?? ''),
                                  BaseButton.small(
                                    tag: e.type == "0"
                                        ? 'group-list-cancel'
                                        : null,
                                    onPressed: () async {
                                      final mmId = e.mmId;
                                      if (mmId == null) return;
                                      await context.push(
                                        '/miners/group/${widget.minerId}/$mmId/checkout',
                                      );
                                      await _refresh();
                                    },
                                    text: e.type == "1" ? 'Group' : 'Cancel',
                                  ),
                                ],
                              ),
                            );
                          },
                        ).toList(),
                      ),
                const SizedBox(height: 20),
                BaseButton(
                  text: 'Create',
                  onPressed: () async {
                    await context
                        .push('/miners/group/${widget.minerId}/create');
                    await _refresh();
                  },
                ),
              ],
            );
          },
        ),
      ),
    );
  }
}
