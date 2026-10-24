import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/member_provider.dart';
import '../utils/utils.dart';
import '../widgets/base_card.dart';
import '../widgets/base_dropdown.dart';
import '../widgets/linear_gradient_background.dart';
import '../widgets/pagination.dart';

class SalesRebateScreen extends StatefulWidget {
  const SalesRebateScreen({super.key});

  @override
  State<SalesRebateScreen> createState() => _SalesRebateScreenState();
}

class _SalesRebateScreenState extends State<SalesRebateScreen> {
  final _perPage = 10;
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
    final record = context.watch<MemberProvider>().salesRebateRecord;
    final total = int.tryParse(record?.pagination?.total ?? '0') ?? 0;
    return total > _perPage;
  }

  Future<void> _fetchData() async {
    setState(() {
      _isLoading = true;
    });

    try {
      await context.read<MemberProvider>().fetchSalesRebateRecord(
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
          title: const Text('Sales Rebate'),
        ),
        body: _isLoading
            ? const Center(child: CircularProgressIndicator())
            : Consumer<MemberProvider>(
                builder: (context, provider, child) {
                  final record = provider.salesRebateRecord;
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
                                        const Text('Rewards'),
                                        const Spacer(),
                                        Text(
                                          '${formatNumber(item.amount)} USDT',
                                          style: const TextStyle(
                                            color: Color(0xFF009C46),
                                            fontWeight: FontWeight.w600,
                                          ),
                                        ),
                                      ],
                                    ),
                                    const SizedBox(height: 16),
                                    Row(
                                      children: [
                                        Text(
                                          example.invalid ?? '',
                                          style: const TextStyle(
                                              color: Colors.grey),
                                        ),
                                      ],
                                    ),
                                    const SizedBox(height: 16),
                                    Container(
                                      width: double.infinity,
                                      height: 1,
                                      color: const Color(0xFF252525),
                                    ),
                                    const SizedBox(height: 16),
                                    Row(
                                      children: [
                                        const Text(
                                          'Date',
                                          style: TextStyle(color: Colors.grey),
                                        ),
                                        const Spacer(),
                                        Text(item.summaryTime ?? ''),
                                      ],
                                    ),
                                    const SizedBox(height: 16),
                                    Row(
                                      children: [
                                        const Text(
                                          'Miner',
                                          style: TextStyle(color: Colors.grey),
                                        ),
                                        const Spacer(),
                                        Text(item.minerName ?? ''),
                                      ],
                                    ),
                                    const SizedBox(height: 16),
                                    Row(
                                      children: [
                                        const Text(
                                          'Account',
                                          style: TextStyle(color: Colors.grey),
                                        ),
                                        const Spacer(),
                                        Expanded(
                                          child: Text(
                                            item.account ?? '',
                                            overflow: TextOverflow.ellipsis,
                                            textAlign: TextAlign.right,
                                          ),
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
