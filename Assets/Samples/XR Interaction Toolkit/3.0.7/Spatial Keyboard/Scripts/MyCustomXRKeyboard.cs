using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;

//
// �Զ���ļ����࣬�̳��� XRKeyboard
// 1. �ṩ ForceSetText(...) ��������
// 2. ��д Open(...) ��������ѡ��
//
public class MyCustomXRKeyboard : XRKeyboard
{
    /// <summary>
    /// ���������������ⲿ�ű����л���ĳ���� InputField ǰ��
    /// �Ѹ� InputField �������ı��������̡�
    /// </summary>
    public void ForceSetText(string newText)
    {
        // XRKeyboard �ڲ��� text ������ protected��
        // ���������ֱ�Ӹ�ֵ������ onTextUpdated �¼���
        text = newText;
    }

    /// <summary>
    /// ��д XRKeyboard.Open(...)��
    /// ������ڼ���������ǰ���Ȱ�Ŀ�� InputField ���ı�ͬ�������̣��������ﴦ��
    /// ��Ҳ���Ը��� XRKeyboardDisplay �� OnInputFieldGainedFocus �е��� ForceSetText��
    /// </summary>
    public override void Open(TMP_InputField inputField, bool observeCharacterLimit = false)
    {
        // �����ϣ���ڵ��� base.Open(...) ֮ǰ���ͽ� inputField ���ı�������
        if (inputField != null)
        {
            ForceSetText(inputField.text);
        }

        // ���û����߼�
        base.Open(inputField, observeCharacterLimit);
    }
}
