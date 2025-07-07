#!/usr/bin/env python3
"""
Training Data Generator for Everwell Healthcare AI
Generates domain-specific conversation data for fine-tuning
"""

import json
import csv
from datetime import datetime
from typing import List, Dict, Tuple

class EverwellTrainingDataGenerator:
    def __init__(self):
        self.medical_qa_pairs = []
        self.conversation_examples = []
        
    def generate_menstrual_cycle_data(self) -> List[Dict]:
        """Generate training data for menstrual cycle questions"""
        
        qa_pairs = [
            {
                "user": "Chu kỳ kinh nguyệt của tôi 32 ngày có bình thường không?",
                "assistant": "Chu kỳ 32 ngày hoàn toàn bình thường! 😊 Chu kỳ kinh nguyệt khỏe mạnh từ 21-35 ngày. Chu kỳ 32 ngày cho thấy hormone của bạn đang hoạt động tốt.\n\n📱 Ứng dụng Everwell có thể giúp bạn theo dõi chu kỳ chính xác và dự đoán ngày kinh nguyệt tiếp theo!\n\n💡 *Lưu ý: Thông tin này chỉ mang tính tham khảo. Hãy tư vấn bác sĩ nếu có bất thường.*"
            },
            {
                "user": "Tại sao tôi có dấu hiệu PMS trước khi đến kỳ?",
                "assistant": "Triệu chứng PMS (hội chứng tiền kinh nguyệt) rất phổ biến! 🌸 Đây là do sự thay đổi hormone estrogen và progesterone trước kỳ kinh.\n\nCác triệu chứng thường gặp:\n• Đau ngực, đầy hơi\n• Thay đổi tâm trạng\n• Mệt mỏi, khó ngủ\n• Thèm ăn\n\n💪 Cách giảm PMS:\n• Tập thể dục nhẹ\n• Hạn chế caffeine, muối\n• Ngủ đủ giấc\n• Quản lý stress\n\n📱 Ghi lại triệu chứng PMS trong Everwell để theo dõi pattern và tìm cách quản lý tốt hơn!\n\n💡 *Nếu PMS quá nặng ảnh hưởng cuộc sống, hãy tư vấn bác sĩ qua tính năng đặt lịch hẹn.*"
            },
            {
                "user": "Làm sao biết mình đang rụng trứng?",
                "assistant": "Có nhiều dấu hiệu giúp nhận biết thời kỳ rụng trứng! 🥚\n\n**Dấu hiệu chính:**\n• 🌡️ Thân nhiệt tăng 0.2-0.5°C\n• 💧 Chất nhầy cổ tử cung trong suốt, giãn như lòng trắng trứng\n• 💓 Đau nhẹ một bên bụng dưới\n• 😍 Tăng ham muốn tình dục\n• ✨ Da sáng hơn, năng lượng tăng\n\n**Thời điểm:** Thường ngày 14 của chu kỳ 28 ngày (có thể từ ngày 11-21)\n\n📱 Tính năng theo dõi rụng trứng trong Everwell sẽ giúp bạn:\n• Ghi nhận triệu chứng hàng ngày\n• Dự đoán ngày rụng trứng\n• Tối ưu cơ hội thụ thai hoặc tránh thai tự nhiên\n\n💡 *Thông tin chỉ tham khảo. Tư vấn bác sĩ để được hướng dẫn chi tiết về kế hoạch sinh sản.*"
            }
        ]
        
        return qa_pairs

    def generate_sti_testing_data(self) -> List[Dict]:
        """Generate training data for STI testing questions"""
        
        qa_pairs = [
            {
                "user": "Tôi cần xét nghiệm STI những gì?",
                "assistant": "Xét nghiệm STI rất quan trọng cho sức khỏe! 🧪 Các xét nghiệm cơ bản gồm:\n\n**Xét nghiệm cần thiết:**\n• HIV/AIDS\n• Giang mai (Syphilis)\n• Lậu (Gonorrhea)\n• Chlamydia\n• Herpes (HSV-1, HSV-2)\n• HPV (Human Papillomavirus)\n• Viêm gan B, C\n\n**Tần suất:** 6-12 tháng/lần cho người có đời sống tình dục\n\n**Thời điểm nên xét nghiệm:**\n• Khi bắt đầu quan hệ với bạn tình mới\n• Có triệu chứng bất thường\n• Bạn tình dương tính với STI\n• Định kỳ để phòng ngừa\n\n🧪 Everwell cung cấp dịch vụ xét nghiệm STI thuận tiện:\n• Xét nghiệm tại nhà\n• Kết quả nhanh, bảo mật\n• Tư vấn trước và sau xét nghiệm\n\n💡 *Xét nghiệm sớm giúp phát hiện và điều trị kịp thời, bảo vệ sức khỏe bản thân và bạn tình.*"
            },
            {
                "user": "Xét nghiệm STI có đau không?",
                "assistant": "Đừng lo lắng! Hầu hết xét nghiệm STI rất đơn giản và ít đau 😊\n\n**Các loại mẫu xét nghiệm:**\n• 🩸 **Máu**: Chỉ đau nhẹ như tiêm vaccine\n• 💧 **Nước tiểu**: Hoàn toàn không đau\n• 🧬 **Dịch âm đạo**: Hơi khó chịu nhưng rất nhanh (< 30 giây)\n• 👄 **Nước bọt**: Hoàn toàn không đau\n\n**Quy trình:**\n• Y tá chuyên nghiệp thực hiện\n• Thời gian ngắn (5-10 phút)\n• Môi trường riêng tư, thoải mái\n\n**Sau xét nghiệm:**\n• Không ảnh hưởng sinh hoạt\n• Kết quả trong 2-5 ngày\n• Tư vấn kết quả miễn phí\n\n🧪 Đặt lịch xét nghiệm STI qua Everwell ngay hôm nay để an tâm về sức khỏe!\n\n💡 *Xét nghiệm định kỳ là biểu hiện của người quan tâm đến sức khỏe bản thân và bạn đời.*"
            }
        ]
        
        return qa_pairs

    def generate_appointment_booking_data(self) -> List[Dict]:
        """Generate training data for appointment booking"""
        
        qa_pairs = [
            {
                "user": "Làm sao đặt lịch hẹn với bác sĩ phụ khoa?",
                "assistant": "Đặt lịch hẹn qua Everwell rất dễ dàng! 👩‍⚕️\n\n**Các bước đặt lịch:**\n1. 📱 Mở ứng dụng Everwell\n2. 🔍 Chọn \"Đặt lịch hẹn\"\n3. 👨‍⚕️ Chọn chuyên khoa: Phụ khoa\n4. 📅 Chọn ngày và giờ phù hợp\n5. ✍️ Mô tả triệu chứng/lý do khám\n6. ✅ Xác nhận đặt lịch\n\n**Loại tư vấn:**\n• 💻 Tư vấn trực tuyến (video call)\n• 🏥 Khám tại phòng mạch\n• 💬 Chat với bác sĩ\n• 📞 Tư vấn qua điện thoại\n\n**Chuyên khoa có sẵn:**\n• Phụ khoa\n• Sản khoa\n• Nội tiết sinh sản\n• Tâm lý học\n• Dinh dưỡng\n\n🕐 **Thời gian:** 8:00 - 20:00 hàng ngày\n💰 **Chi phí:** Minh bạch, được thông báo trước\n\n📞 Cần hỗ trợ đặt lịch? Liên hệ hotline Everwell!"
            },
            {
                "user": "Tôi có thể tư vấn bác sĩ về chu kỳ kinh nguyệt bất thường không?",
                "assistant": "Tất nhiên! Chu kỳ kinh nguyệt bất thường cần được bác sĩ đánh giá kỹ 📋\n\n**Triệu chứng nên thăm khám:**\n• Chu kỳ < 21 ngày hoặc > 35 ngày\n• Kinh nguyệt kéo dài > 7 ngày\n• Ra máu ngoài chu kỳ\n• Đau bụng dưới dữ dội\n• Lượng kinh quá nhiều/ít bất thường\n\n**Bác sĩ sẽ:**\n• 🔍 Hỏi về lịch sử kinh nguyệt\n• 🩺 Khám lâm sàng (nếu cần)\n• 🧪 Chỉ định xét nghiệm hormone\n• 📋 Đưa ra chẩn đoán và phương án điều trị\n\n**Chuẩn bị trước khi đến:**\n• 📱 Ghi chép chu kỳ trong Everwell (ít nhất 3 tháng)\n• 📝 Liệt kê triệu chứng\n• 💊 Danh sách thuốc đang dùng\n• 🧬 Tiền sử gia đình (nếu có)\n\n👩‍⚕️ Đặt lịch tư vấn với bác sĩ phụ khoa qua Everwell ngay để được hỗ trợ tốt nhất!\n\n⚠️ *Nếu đau dữ dội đột ngột, hãy gọi 115 hoặc đến bệnh viện ngay!*"
            }
        ]
        
        return qa_pairs

    def generate_general_health_data(self) -> List[Dict]:
        """Generate training data for general health questions"""
        
        qa_pairs = [
            {
                "user": "Ứng dụng Everwell có những tính năng gì?",
                "assistant": "Everwell Healthcare cung cấp giải pháp chăm sóc sức khỏe phụ nữ toàn diện! 🌸\n\n**Tính năng chính:**\n📅 **Theo dõi chu kỳ kinh nguyệt**\n• Dự đoán chu kỳ và rụng trứng\n• Ghi nhận triệu chứng PMS\n• Nhắc nhở thuốc tránh thai\n\n🧪 **Xét nghiệm STI**\n• Đặt lịch xét nghiệm tại nhà\n• Kết quả nhanh, bảo mật\n• Tư vấn trước/sau xét nghiệm\n\n👩‍⚕️ **Tư vấn y tế**\n• Chat/video call với bác sĩ\n• Đặt lịch hẹn linh hoạt\n• Chuyên khoa đa dạng\n\n💪 **Theo dõi sức khỏe**\n• Cân nặng, huyết áp\n• Giấc ngủ, tâm trạng\n• Hoạt động thể chất\n\n🔔 **Thông báo thông minh**\n• Nhắc nhở khám định kỳ\n• Cảnh báo bất thường\n• Tips sức khỏe hàng ngày\n\n🎯 **Mục tiêu:** Đồng hành cùng phụ nữ Việt Nam chăm sóc sức khỏe một cách khoa học và hiện đại!\n\n📱 Tải Everwell ngay để trải nghiệm!"
            },
            {
                "user": "Tôi lo lắng về sức khỏe sinh sản",
                "assistant": "Cảm xúc lo lắng về sức khỏe sinh sản là hoàn toàn bình thường! 💚 Điều quan trọng là bạn đã quan tâm đến sức khỏe của mình.\n\n**Những điều bạn có thể làm:**\n\n🔍 **Hiểu biết cơ bản:**\n• Tìm hiểu về chu kỳ kinh nguyệt\n• Nhận biết dấu hiệu bất thường\n• Học về phòng tránh STI\n\n📋 **Kiểm tra định kỳ:**\n• Khám phụ khoa 6-12 tháng/lần\n• Xét nghiệm STI theo khuyến cáo\n• Theo dõi chu kỳ hàng ngày\n\n🥗 **Lối sống lành mạnh:**\n• Ăn uống cân bằng\n• Tập thể dục đều đặn\n• Quản lý stress\n• Ngủ đủ giấc\n\n💬 **Tìm sự hỗ trợ:**\n• Trao đổi với bạn bè, gia đình\n• Tham gia cộng đồng sức khỏe\n• Tư vấn bác sĩ khi cần\n\n👩‍⚕️ Everwell luôn sẵn sàng hỗ trợ bạn:\n• Chat với bác sĩ 24/7\n• Tài liệu sức khỏe đáng tin cậy\n• Cộng đồng hỗ trợ tích cực\n\n💡 *Nhớ rằng: Quan tâm sức khỏe sinh sản là dấu hiệu của người phụ nữ hiện đại và thông minh!*"
            }
        ]
        
        return qa_pairs

    def generate_safety_data(self) -> List[Dict]:
        """Generate training data for safety and emergency situations"""
        
        qa_pairs = [
            {
                "user": "Tôi đau bụng dưới dữ dội và ra máu nhiều",
                "assistant": "⚠️ **ĐÂY LÀ TÌNH HUỐNG CẤP CỨU!**\n\n🚨 **Hành động ngay lập tức:**\n• Gọi 115 hoặc đến bệnh viện gần nhất NGAY\n• Không được trì hoãn!\n• Thông báo cho người thân\n\n**Triệu chứng nghiêm trọng:**\n• Đau bụng dữ dội đột ngột\n• Ra máu âm đạo nhiều bất thường\n• Có thể là dấu hiệu:\n  - Thai ngoài tử cung vỡ\n  - Chảy máu buồng trứng\n  - Sảy thai\n  - Các cấp cứu phụ khoa khác\n\n📞 **Số điện thoại cấp cứu:**\n• 115 (Cấp cứu quốc gia)\n• Bệnh viện gần nhất\n\n⏰ **THỜI GIAN LÀ VÀNG - ĐỪNG CHỜ ĐỢI!**\n\n💡 *Everwell khuyến cáo mạnh mẽ: Trong tình huống này, hãy tìm kiếm chăm sóc y tế cấp cứu ngay lập tức. Đây không phải lúc để tự chẩn đoán hoặc chờ đợi.*"
            },
            {
                "user": "Khi nào tôi cần đi khám gấp?",
                "assistant": "Dấu hiệu cần khám gấp rất quan trọng! 🚨 Hãy ghi nhớ:\n\n⚠️ **CẤP CỨU NGAY (Gọi 115):**\n• Đau bụng dưới dữ dội đột ngột\n• Ra máu âm đạo nhiều bất thường\n• Sốt cao (>39°C) kèm đau bụng\n• Ngất xỉu, chóng mặt nặng\n• Khó thở, đau ngực\n• Không thể đi lại được\n\n📅 **ĐẶT LỊCH KHÁM TRONG 24-48H:**\n• Chu kỳ kinh nguyệt đột ngột thay đổi\n• Đau khi quan hệ tình dục\n• Ngứa, tiết dịch có mùi lạ\n• Đau bụng kéo dài >3 ngày\n• Phát ban, mụn nước ở bộ phận sinh dục\n• Thay đổi tâm trạng nghiêm trọng\n\n📋 **KHÁM ĐỊNH KỲ:**\n• Khám phụ khoa 6-12 tháng/lần\n• Xét nghiệm STI theo lịch\n• Tầm soát ung thư cổ tử cung\n\n👩‍⚕️ **Everwell hỗ trợ:**\n• Đặt lịch khám khẩn cấp\n• Tư vấn triệu chứng 24/7\n• Hướng dẫn sơ cứu qua điện thoại\n\n💡 *Luôn tin vào cảm giác của bản thân - khi có gì đó \"không đúng\", hãy tìm kiếm sự giúp đỡ y tế!*"
            }
        ]
        
        return qa_pairs

    def compile_training_data(self) -> List[Dict]:
        """Compile all training data"""
        
        all_data = []
        all_data.extend(self.generate_menstrual_cycle_data())
        all_data.extend(self.generate_sti_testing_data())
        all_data.extend(self.generate_appointment_booking_data())
        all_data.extend(self.generate_general_health_data())
        all_data.extend(self.generate_safety_data())
        
        # Add metadata
        for item in all_data:
            item['timestamp'] = datetime.now().isoformat()
            item['source'] = 'everwell_healthcare'
            item['domain'] = 'women_health'
            item['language'] = 'vietnamese'
            
        return all_data

    def save_to_jsonl(self, data: List[Dict], filename: str):
        """Save training data to JSONL format for model training"""
        
        with open(filename, 'w', encoding='utf-8') as f:
            for item in data:
                # Format for model training
                training_example = {
                    "messages": [
                        {"role": "user", "content": item["user"]},
                        {"role": "assistant", "content": item["assistant"]}
                    ],
                    "metadata": {
                        "source": item["source"],
                        "domain": item["domain"],
                        "language": item["language"],
                        "timestamp": item["timestamp"]
                    }
                }
                f.write(json.dumps(training_example, ensure_ascii=False) + '\n')

    def save_to_csv(self, data: List[Dict], filename: str):
        """Save training data to CSV format"""
        
        with open(filename, 'w', newline='', encoding='utf-8') as f:
            writer = csv.writer(f)
            writer.writerow(['user_input', 'assistant_response', 'source', 'domain', 'language', 'timestamp'])
            
            for item in data:
                writer.writerow([
                    item['user'],
                    item['assistant'],
                    item['source'],
                    item['domain'],
                    item['language'],
                    item['timestamp']
                ])

def main():
    """Main function to generate training data"""
    
    generator = EverwellTrainingDataGenerator()
    training_data = generator.compile_training_data()
    
    # Save in multiple formats
    generator.save_to_jsonl(training_data, 'everwell_training_data.jsonl')
    generator.save_to_csv(training_data, 'everwell_training_data.csv')
    
    # Save knowledge base
    with open('everwell_knowledge_base.json', 'r', encoding='utf-8') as f:
        knowledge_base = json.load(f)
    
    # Generate summary
    summary = {
        'total_examples': len(training_data),
        'categories': {
            'menstrual_health': len([d for d in training_data if 'chu kỳ' in d['user'].lower() or 'kinh nguyệt' in d['user'].lower()]),
            'sti_testing': len([d for d in training_data if 'xét nghiệm' in d['user'].lower() or 'sti' in d['user'].lower()]),
            'appointments': len([d for d in training_data if 'đặt lịch' in d['user'].lower() or 'bác sĩ' in d['user'].lower()]),
            'general_health': len([d for d in training_data if 'everwell' in d['assistant'].lower()]),
            'safety_emergency': len([d for d in training_data if 'cấp cứu' in d['assistant'].lower() or '115' in d['assistant'].lower()])
        },
        'generated_at': datetime.now().isoformat(),
        'language': 'vietnamese',
        'domain': 'women_healthcare'
    }
    
    with open('training_summary.json', 'w', encoding='utf-8') as f:
        json.dump(summary, f, ensure_ascii=False, indent=2)
    
    print(f"✅ Generated {len(training_data)} training examples")
    print(f"📁 Saved to: everwell_training_data.jsonl, everwell_training_data.csv")
    print(f"📊 Summary saved to: training_summary.json")
    print("\n🚀 Ready for model training!")

if __name__ == "__main__":
    main() 