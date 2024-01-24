using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Database : MonoBehaviour
{
    public string GetRandomSubject()
    {
        string[] subjects = { "����", "����", "����" };
        int randomNumber = Random.Range(0, 3);
        return subjects[randomNumber];
    }

    public string GetRandomWord(string subject)
    {
        if (subject.Equals("����"))
        {
            string[] words = {
                "�ܹ���", "¥���", "�Ľ�Ÿ", "Ÿ��", "�ʹ�", "ī��", "��ġ������", "��ġ�", "������", "����",
                "�Ұ���", "������", "����", "�ø�", "���", "������", "������", "�ع���", "�Ұ��ⱹ��", "�������",
                "������", "����", "����", "�ܹ���", "ġŲ", "������ũ", "�̿���", "���뱹", "���", "����",
                "����ġ", "�������", "���", "���Ƕ��̽�", "�����屹", "������", "������", "����", "����", "������",
                "�粿ġ", "������", "�ſ����", "�����", "�ߺ�����", "�ӹ�", "������", "�ſ���", "��������", "�Ʊ���",
                "������", "��ġ��", "����", "�����", "���������", "���ĵκ�", "�̸�", "���ø�", "����ø�", "��ȸ",
                "��¡���", "ȸ����", "�޲ٹ̺���", "��붱", "�佺Ʈ", "�����ֹ���", "���ġŲ", "��ġ�˹�", "���", "�ᱹ��",
                "��", "���丮��", "����", "�ұ���", "�簥��", "����", "��â", "��ٷο�", "�̸�", "�ʸ����߾����",
                "�ҸӸ�����", "�߰���", "�ɰ���", "�����", "�޲ٹ���", "��������", "����ġ", "����", "������", "������",
                "�����", "����Ұ���", "�ع���������", "�����к���", "������", "���縮", "�ı�ġ", "�������", "�����ʹ�", "��ġ���ʹ�"
            };
            int randomNumber = Random.Range(0, words.Length - 1);
            return words[randomNumber];
        }
        else if (subject.Equals("����"))
        {
            string[] words = {
                "����", "�ڳ���", "�ڻԼ�", "�⸰", "ȣ����", "ġŸ", "ǥ��", "����", "����", "�罿",
                "Ļ�ŷ�", "������", "������", "����", "������", "�ξ���", "���", "�ϸ�", "�ھ˶�", "�ٴټ�",
                "��", "������", "������", "�ܽ���", "�䳢", "�ź���", "�̱��Ƴ�", "������", "�Ǵ�", "�Ǿ�",
                "��", "��", "����", "��", "��", "����", "������ġ", "�ٴٰź�", "�ظ�", "��Ÿ",
                "�����", "����", "�ĸ�", "���ڸ�", "�Ź�", "��", "����", "�޶ѱ�", "������", "����",
                "��ī", "�޹���", "ī�᷹��", "�δ���", "�޺��Ƹ�", "����", "�ٴٻ���", "�ϱذ�", "��ȣ", "�ٴ�ǥ��",
                "���ٸ�", "û������", "��������", "�ϱؿ���", "������ź", "����ī", "Ǫ��", "ġ�Ϳ�", "���۵�", "��Ƽ��",
                "��������", "��ttweiler", "�ùٰ�", "�۱�", "�������׸���", "����ġ�ҵ���", "������ġ", "�䳢", "�ܽ���", "����Ǳ�",
                "�޹���", "�ݺؾ�", "���ڳ�ũ��", "ȫ��", "�Ǵ�����", "����̻��", "���齺��", "�źϼ�", "������", "�����ظ�"
                // ������ ������ ����ؼ� �߰��ϼ���
            };
            int randomNumber = Random.Range(0, words.Length - 1);
            return words[randomNumber];
        }
        else if (subject.Equals("����"))
        {
            string[] words = {
                "�ǻ�", "��ȣ��", "������", "������", "�ҹ��", "���డ", "���α׷���", "�����̳�", "�����Ͼ�", "ȸ���",
                "��ȣ��", "�۰�", "���", "����", "����", "�丮��", "û�Һ�", "��۱��", "�Ǹſ�", "�Ƿ���",
                "�̿��", "�������̳�", "���", "������", "�����", "���θ���Ʈ", "������", "������", "������", "����",
                "��ȸ������", "�����", "�濵��", "������", "IT������", "��������", "�����ͺм���", "�뿪��", "������", "���׸�������̳�",
                "����� ������", "������ �����", "���� ����", "������ ��ġ", "���Ϸ�", "����� ����", "��������� ����", "ȭ��", "�����", "�ۼų� Ʈ���̳�",
                "�������ε༭", "�ǾƴϽ�Ʈ", "�ٸ���Ÿ", "����ŴϽ�Ʈ", "������", "������ Ʃ��", "��ǻ�� �ϵ���� �����Ͼ�", "�������డ", "�־� �����̳�", "�ݼӰ�����",
                "�������� ������", "�ǰ��ڵ������", "ȭ�а�����", "�ɸ�ġ���", "��ȹ��", "ȣ�� �Ŵ���", "���̿ø��Ͻ�Ʈ", "����׷���", "�ڵ��� �����̳�", "ȭ��",
                "�Ĺ�����", "���ǻ�", "���� ����", "�Ҽ���", "������ ���", "���� ����", "�����۰�", "��������", "���� �����", "�ҹ���",
                "�Ȱ� �ǻ�", "�����׶��ǽ�Ʈ", "�ݵ�ü �����Ͼ�", "�ǳ����డ", "����������", "��ǻ�� ���α׷���", "�긲�����", "���ġ���", "�̼��� ����", "�ҹ���"
                // ������ ������ ����ؼ� �߰��ϼ���
            };
            int randomNumber = Random.Range(0, words.Length - 1);
            return words[randomNumber];
        }
        return "�ش���������";
    }
}
