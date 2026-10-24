import 'package:collection/collection.dart';
import 'package:flutter/material.dart';

import '../themes/app_colors.dart';
import '../utils/faq_list.dart';
import '../widgets/linear_gradient_background.dart';

class FAQDetailScreen extends StatelessWidget {
  final String faqId;
  const FAQDetailScreen({super.key, required this.faqId});

  @override
  Widget build(BuildContext context) {
    final faq = faqList
        .expand((e) => e['items'])
        .firstWhereOrNull((e) => e['id'] == int.parse(faqId));

    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('FAQ'),
        ),
        body: SingleChildScrollView(
          padding: const EdgeInsets.all(20),
          child: Column(
            children: [
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Container(
                    width: 20,
                    height: 20,
                    decoration: BoxDecoration(
                      color: const Color(0xff3B363F),
                      borderRadius: BorderRadius.circular(4),
                    ),
                    child: const Center(child: Text("Q")),
                  ),
                  const SizedBox(width: 10),
                  Expanded(child: Text(faq == null ? '-' : faq['question'])),
                ],
              ),
              const SizedBox(height: 16),
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Container(
                    width: 20,
                    height: 20,
                    decoration: BoxDecoration(
                      color: AppColors.primaryColor,
                      borderRadius: BorderRadius.circular(4),
                    ),
                    child: const Center(
                      child: Text(
                        "A",
                        style: TextStyle(color: Colors.black),
                      ),
                    ),
                  ),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Text(
                      faq == null ? '-' : faq['answer'],
                      style: const TextStyle(
                        color: Color(0xFFa3a3a3),
                      ),
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
