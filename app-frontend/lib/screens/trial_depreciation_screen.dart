import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/my_miner.dart';
import '../providers/miner_provider.dart';
import '../utils/utils.dart';
import '../widgets/base_card.dart';
import '../widgets/base_dropdown.dart';
import '../widgets/base_text_field.dart';
import '../widgets/linear_gradient_background.dart';

class TrialDepreciationScreen extends StatefulWidget {
  const TrialDepreciationScreen({super.key});

  @override
  State<TrialDepreciationScreen> createState() =>
      _TrialDepreciationScreenState();
}

class _TrialDepreciationScreenState extends State<TrialDepreciationScreen> {
  final TextEditingController _controller = TextEditingController();
  late Future<void> _future;
  MyMiner? _selectedMiner;
  DateTime? _selectedDate;

  DateTime get _datePickerFirstDate {
    final sellbackStart = _selectedMiner?.sellbackStart;
    if (sellbackStart == null) return DateTime.now();
    final parsedDate = DateTime.tryParse(sellbackStart);
    if (parsedDate == null) return DateTime.now();
    final startDate = parsedDate.add(const Duration(days: 1));
    final now = DateTime.now();
    return now.isAfter(startDate) ? now : startDate;
  }

  DateTime get _datePickerLastDate {
    return _datePickerFirstDate.add(const Duration(days: 90));
  }

  @override
  void initState() {
    _future = context.read<MinerProvider>().fetchMyPurchaseMiners().then((_) {
      if (!mounted) return;
      if (context.read<MinerProvider>().myPurchaseMiners.isNotEmpty) {
        final first = context.read<MinerProvider>().myPurchaseMiners.first;
        _setMiner(first);
      }
    });
    super.initState();
  }

  void _setMiner(MyMiner? miner) {
    setState(() {
      _selectedMiner = miner;
      _setDate(_datePickerFirstDate);
    });
  }

  Future<void> _setDate(DateTime date) async {
    setState(() {
      _selectedDate = date;
    });
    await _updateAmount();
  }

  Future<void> _updateAmount() async {
    setState(() {
      _controller.text = "Calculating...";
    });
    try {
      final mmId = _selectedMiner?.mmId;
      final date = _selectedDate;
      if (mmId == null || date == null) {
        throw Exception();
      }
      final result = await context.read<MinerProvider>().fetchTrialDepreciation(
            mmId: mmId,
            date: date,
          );
      if (!mounted) return;

      setState(() {
        _controller.text = formatNumber(result);
      });
    } catch (e) {
      setState(() {
        _controller.text = "Error";
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Trial Depreciation'),
        ),
        body: FutureBuilder(
          future: _future,
          builder: (context, snapshot) {
            if (snapshot.connectionState != ConnectionState.done) {
              return const Center(child: CircularProgressIndicator());
            }

            if (snapshot.hasError) {
              return Center(child: Text(snapshot.error.toString()));
            }

            if (context.read<MinerProvider>().myPurchaseMiners.isEmpty) {
              return const Center(child: Text('No data'));
            }
            return SingleChildScrollView(
              padding: const EdgeInsets.all(20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  BaseDropdown(
                    labelText: "Miner",
                    value: _selectedMiner,
                    items: context
                        .read<MinerProvider>()
                        .myPurchaseMiners
                        .map(
                          (e) => DropdownMenuItem(
                            value: e,
                            child: Text(e.name ?? ""),
                          ),
                        )
                        .toList(),
                    onChanged: (p0) {
                      _setMiner(p0 as MyMiner?);
                    },
                  ),
                  const SizedBox(height: 20),
                  const Text('Return Date'),
                  const SizedBox(height: 4),
                  BaseCard(
                    width: double.infinity,
                    padding: const EdgeInsets.all(14),
                    child: Text(_selectedDate?.toString().split(" ")[0] ?? ""),
                    onTap: () async {
                      final date = await showDatePicker(
                        context: context,
                        initialDate: _selectedDate,
                        firstDate: _datePickerFirstDate,
                        lastDate: _datePickerLastDate,
                        locale: const Locale('en'),
                      );
                      if (date == null) return;
                      _setDate(date);
                    },
                  ),
                  const SizedBox(height: 20),
                  BaseTextField(
                    controller: _controller,
                    labelText: "Depreciation amount",
                    readOnly: true,
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
