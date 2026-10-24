import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/fund_record.dart';
import '../providers/wallet_provider.dart';
import '../utils/utils.dart';
import '../widgets/base_card.dart';
import '../widgets/base_dropdown.dart';
import '../widgets/linear_gradient_background.dart';
import '../widgets/pagination.dart';

class FundRecordScreen extends StatefulWidget {
  const FundRecordScreen({super.key});

  @override
  State<FundRecordScreen> createState() => _FundRecordScreenState();
}

class _FundRecordScreenState extends State<FundRecordScreen> {
  final int _perPage = 10;
  final dayOptions = [
    {'label': '1 Day', 'value': 1},
    {'label': '3 Days', 'value': 3},
    {'label': '7 Days', 'value': 7},
    {'label': '30 Days', 'value': 30},
    {'label': '100 Days', 'value': 100},
  ];
  List<FundRecordType> typeOptions = [
    FundRecordType(name: 'All', id: '0'),
  ];
  Map<String, dynamic>? _selectedDay;
  FundRecordType? _selectedType;
  int _page = 1;
  bool _isLoading = false;

  bool get _showPagination {
    final record = context.watch<WalletProvider>().fundRecord;
    final total = int.tryParse(record?.pagination?.total ?? '0') ?? 0;
    return total > _perPage;
  }

  Future<void> _fetchData() async {
    setState(() {
      _isLoading = true;
    });

    try {
      await context.read<WalletProvider>().fetchFundRecord(
            days: _selectedDay?['value'] ?? 1,
            type: _selectedType?.id ?? '0',
            page: _page,
            perPage: _perPage,
          );
    } finally {
      setState(() {
        _isLoading = false;
      });
    }
  }

  Future<void> _fetchType() async {
    await context.read<WalletProvider>().fetchFundRecordType();
    setState(() {
      typeOptions.addAll(context.read<WalletProvider>().fundRecordTypeList);
    });
  }

  @override
  void initState() {
    _selectedDay = dayOptions.first;
    _selectedType = typeOptions.first;
    _fetchType();
    _fetchData();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Fund Records'),
        ),
        body: _isLoading
            ? const Center(child: CircularProgressIndicator())
            : Consumer<WalletProvider>(
                builder: (context, walletProvider, child) {
                  final record = walletProvider.fundRecord;
                  final list = record?.list ?? [];
                  return SingleChildScrollView(
                    padding: const EdgeInsets.all(20),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        SingleChildScrollView(
                          scrollDirection: Axis.horizontal,
                          child: Row(
                            children: [
                              BaseDropdown.small(
                                value: _selectedType,
                                items: typeOptions
                                    .map(
                                      (e) => DropdownMenuItem(
                                        value: e,
                                        child: Text(e.name ?? ''),
                                      ),
                                    )
                                    .toList(),
                                onChanged: (value) {
                                  setState(() {
                                    _selectedType = value;
                                    _page = 1;
                                  });
                                  _fetchData();
                                },
                              ),
                              const SizedBox(width: 16),
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
                            (item) => Column(
                              children: [
                                const SizedBox(height: 20),
                                BaseCard(
                                  padding: const EdgeInsets.symmetric(
                                    horizontal: 16,
                                    vertical: 20,
                                  ),
                                  width: double.infinity,
                                  child: Column(
                                    children: [
                                      Row(
                                        children: [
                                          Text(
                                            item.transactionTypeName ?? '',
                                          ),
                                          const Spacer(),
                                          Text(
                                            '${formatNumber(item.point)} ${item.walletTypeName ?? ''}',
                                            style: TextStyle(
                                              color:
                                                  item.point?.startsWith('-') ??
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
                                            style:
                                                TextStyle(color: Colors.grey),
                                          ),
                                          const Spacer(),
                                          Text(item.createdAt ?? ''),
                                        ],
                                      ),
                                      const SizedBox(height: 16),
                                      Row(
                                        children: [
                                          const Text(
                                            'Before balance',
                                            style:
                                                TextStyle(color: Colors.grey),
                                          ),
                                          const Spacer(),
                                          Text(
                                            '${formatNumber(item.beforeBalance)} ${item.walletTypeName ?? ''}',
                                          ),
                                        ],
                                      ),
                                      const SizedBox(height: 16),
                                      Row(
                                        children: [
                                          const Text(
                                            'After balance',
                                            style:
                                                TextStyle(color: Colors.grey),
                                          ),
                                          const Spacer(),
                                          Text(
                                            '${formatNumber(item.afterBalance)} ${item.walletTypeName ?? ''}',
                                          ),
                                        ],
                                      ),
                                      if (item.remark != null &&
                                          item.remark!.isNotEmpty)
                                        const SizedBox(height: 16),
                                      if (item.remark != null &&
                                          item.remark!.isNotEmpty)
                                        Row(
                                          children: [
                                            const Text(
                                              'Remark',
                                              style:
                                                  TextStyle(color: Colors.grey),
                                            ),
                                            const Spacer(),
                                            Text(item.remark ?? ''),
                                          ],
                                        ),
                                    ],
                                  ),
                                ),
                              ],
                            ),
                          ),
                        const SizedBox(height: 20),
                        if (_showPagination)
                          Pagination(
                            total: int.tryParse(
                                    record?.pagination?.total ?? '0') ??
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
                  );
                },
              ),
      ),
    );
  }
}
