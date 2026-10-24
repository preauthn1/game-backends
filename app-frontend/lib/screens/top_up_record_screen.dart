import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/wallet_provider.dart';
import '../utils/utils.dart';
import '../widgets/base_card.dart';
import '../widgets/base_dropdown.dart';
import '../widgets/linear_gradient_background.dart';
import '../widgets/pagination.dart';

class TopUpRecordScreen extends StatefulWidget {
  const TopUpRecordScreen({super.key});

  @override
  State<TopUpRecordScreen> createState() => _TopUpRecordScreenState();
}

class _TopUpRecordScreenState extends State<TopUpRecordScreen> {
  final int _perPage = 10;
  final dayOptions = [
    {'label': '1 Day', 'value': 1},
    {'label': '3 Days', 'value': 3},
    {'label': '7 Days', 'value': 7},
    {'label': '30 Days', 'value': 30},
    {'label': '100 Days', 'value': 100},
  ];
  Map<String, dynamic>? _selectedDay;
  int _page = 1;
  bool _isLoading = false;

  bool get _showPagination {
    final record = context.watch<WalletProvider>().topUpRecord;
    final total = int.tryParse(record?.pagination?.total ?? '0') ?? 0;
    return total > _perPage;
  }

  Future<void> _fetchData() async {
    setState(() {
      _isLoading = true;
    });

    try {
      await context.read<WalletProvider>().fetchTopUpRecord(
            days: _selectedDay?['value'] ?? 1,
            page: _page,
            perPage: _perPage,
          );
    } finally {
      setState(() {
        _isLoading = false;
      });
    }
  }

  @override
  void initState() {
    _selectedDay = dayOptions.first;
    _fetchData();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Deposit Details'),
        ),
        body: _isLoading
            ? const Center(child: CircularProgressIndicator())
            : Consumer<WalletProvider>(
                builder: (context, walletProvider, child) {
                  final record = walletProvider.topUpRecord;
                  final list = record?.list ?? [];
                  return SingleChildScrollView(
                    padding: const EdgeInsets.all(20),
                    child: SafeArea(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            children: [
                              BaseDropdown.small(
                                value: _selectedDay,
                                items: dayOptions
                                    .map(
                                      (e) => DropdownMenuItem(
                                        value: e,
                                        child: Text(e['label'] as String),
                                      ),
                                    )
                                    .toList(),
                                onChanged: (value) {
                                  setState(() {
                                    _selectedDay = value;
                                  });
                                  _fetchData();
                                },
                              ),
                            ],
                          ),
                          if (list.isEmpty)
                            const Padding(
                              padding: EdgeInsets.all(16.0),
                              child: Center(
                                child: Text('No data found.'),
                              ),
                            ),
                          if (list.isNotEmpty)
                            ...list.map(
                              (item) => BaseCard(
                                padding: const EdgeInsets.symmetric(
                                  horizontal: 16,
                                  vertical: 20,
                                ),
                                margin:
                                    const EdgeInsets.symmetric(vertical: 10),
                                width: double.infinity,
                                child: Column(
                                  children: [
                                    Row(
                                      children: [
                                        Text(
                                          'Order ID: ${item.orderNo}',
                                        ),
                                        const Spacer(),
                                        Text(
                                          item.status ?? '',
                                          style: TextStyle(
                                            color:
                                                item.amount?.startsWith('-') ??
                                                        false
                                                    ? const Color(0xFFff3352)
                                                    : const Color(0xFF009C46),
                                            fontWeight: FontWeight.w600,
                                          ),
                                        ),
                                      ],
                                    ),
                                    const SizedBox(height: 16),
                                    Row(
                                      children: [
                                        const Text(
                                          'Date',
                                          style: TextStyle(color: Colors.grey),
                                        ),
                                        const Spacer(),
                                        Text(item.createdAt ?? ''),
                                      ],
                                    ),
                                    const SizedBox(height: 16),
                                    Row(
                                      children: [
                                        const Text(
                                          'Deposit Currency',
                                          style: TextStyle(color: Colors.grey),
                                        ),
                                        const Spacer(),
                                        Text(item.currencyName ?? '-'),
                                      ],
                                    ),
                                    const SizedBox(height: 16),
                                    Row(
                                      children: [
                                        const Text(
                                          'Amount',
                                          style: TextStyle(color: Colors.grey),
                                        ),
                                        const Spacer(),
                                        Text(formatNumber(item.amount)),
                                      ],
                                    ),
                                    const SizedBox(height: 16),
                                    Row(
                                      children: [
                                        const Text(
                                          'Exchange Rate',
                                          style: TextStyle(color: Colors.grey),
                                        ),
                                        const Spacer(),
                                        Text(formatNumber(item.exchangeRate)),
                                      ],
                                    ),
                                    const SizedBox(height: 16),
                                    Row(
                                      children: [
                                        const Text(
                                          'Point',
                                          style: TextStyle(color: Colors.grey),
                                        ),
                                        const Spacer(),
                                        Text(
                                          formatNumber(item.point),
                                        ),
                                      ],
                                    ),
                                    const SizedBox(height: 16),
                                    Row(
                                      children: [
                                        const Text(
                                          'After Balance',
                                          style: TextStyle(color: Colors.grey),
                                        ),
                                        const Spacer(),
                                        Text(
                                          formatNumber(item.afterBalance),
                                        ),
                                      ],
                                    ),
                                  ],
                                ),
                              ),
                            ),
                          const SizedBox(height: 20),
                          if (_showPagination)
                            Pagination(
                              total: int.tryParse(
                                    record?.pagination?.total ?? '0',
                                  ) ??
                                  0,
                              perPage: _perPage,
                              currentPage: _page,
                              onPageChanged: (page) {
                                setState(() {
                                  _page = page;
                                });
                                _fetchData();
                              },
                            ),
                        ],
                      ),
                    ),
                  );
                },
              ),
      ),
    );
  }
}
