using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class EnvelopeListBox : Clickable {

        public List<Envelope> List { get; private set; }
        EnvelopeListItem[] items;
        public int SelectedIndex { get; set; }
        public Envelope SelectedItem { get { return SelectedIndex >= 0 ? List[SelectedIndex] : null; } }
        DropdownButton addEnvelopeButton;
        Envelope.EnvelopeType[] remainingEnvelopes;

        public EnvelopeListBox(int x, int y, int height, Element parent) {
            this.x = x;
            this.y = y;
            width = 94;
            this.height = height;
            SetParent(parent);
            items = new EnvelopeListItem[Enum.GetValues(typeof(Envelope.EnvelopeType)).Length];
            for (int i = 0; i < items.Length; ++i) {
                items[i] = new EnvelopeListItem(1, 1 + i * 16, this);
            }
            addEnvelopeButton = new DropdownButton("Add Envelope", 0, height + 2, this);
        }

        public void Intialize(List<Envelope> listOfEnvelopes) {
            if (listOfEnvelopes.Count > 0) {
                SelectedIndex = 0;
            }
            else {
                SelectedIndex = -1;
            }
            List = listOfEnvelopes;
            UpdateRemainingEnvelopes();
        }

        public void Update() {
            for (int i = items.Length - 1; i >= 0; --i) {
                if (i < List.Count) {
                    items[i].Envelope = List[i];
                    items[i].Update();
                    if (items[i].WasClickedOnToSelect) {
                        SelectedIndex = i;
                    }
                    if (items[i].WasClickedOnToDelete) {
                        List.RemoveAt(i);
                        Audio.ChannelManager.previewChannel.Reset();
                        App.CurrentModule.SetDirty();
                        UpdateRemainingEnvelopes();
                        if (SelectedIndex >= List.Count)
                            SelectedIndex--;
                    }
                }
                else {
                    items[i].Envelope = null;
                }
            }
            if (List.Count > 1) {
                if (Input.GetKeyRepeat(Keys.Up, KeyModifier.None)) {
                    SelectedIndex--;
                    if (SelectedIndex < 0)
                        SelectedIndex = 0;
                }
                if (Input.GetKeyRepeat(Keys.Down, KeyModifier.None)) {
                    SelectedIndex++;
                    if (SelectedIndex > List.Count - 1)
                        SelectedIndex = List.Count - 1;
                }
            }
            //addEnvelopeButton.width = width - 2;
            //addEnvelopeButton.y = (List.Count - 1) * 16 + 17;
            addEnvelopeButton.enabled = remainingEnvelopes.Length > 0;
            addEnvelopeButton.Update();
            if (addEnvelopeButton.SelectedAnItem) {
                AddEnvelope(remainingEnvelopes[addEnvelopeButton.SelectedIndex]);
                Audio.ChannelManager.previewChannel.Reset();
                App.CurrentModule.SetDirty();
            }


        }
        void UpdateRemainingEnvelopes() {
            List<Envelope.EnvelopeType> list = new List<Envelope.EnvelopeType>();

            foreach (Envelope.EnvelopeType type in Enum.GetValues(typeof(Envelope.EnvelopeType))) {
                bool hasEnvelope = false;
                foreach (Envelope envelope in List) {
                    if (envelope.Type == type) {
                        hasEnvelope = true;
                        break;
                    }
                }
                if (!hasEnvelope) {
                    list.Add(type);
                }
            }
            remainingEnvelopes = list.ToArray();
            string[] names = new string[remainingEnvelopes.Length];
            for (int i = 0; i < names.Length; i++) {
                names[i] = "Add " + Envelope.GetName(remainingEnvelopes[i]) + " envelope";
            }
            addEnvelopeButton.SetMenuItems(names);
        }

        void AddEnvelope(Envelope.EnvelopeType type) {
            foreach (Envelope envelope in List) {
                if (envelope.Type == type) {
                    return;
                }
            }
            int index = 0;
            while (index < List.Count && List[index].Type < type) {
                index++;
            }
            if (index >= List.Count) {
                List.Add(new Envelope(type));
                SelectedIndex = List.Count - 1;

            }
            else {
                List.Insert(index, new Envelope(type));
                SelectedIndex = index;
            }
            UpdateRemainingEnvelopes();
        }



        public void Draw() {
            //DrawRect(0, 0, width, height, new Color(192, 195, 212));
            Write("Envelopes (" + List.Count + "/" + items.Length + ")", 0, -10, UIColors.label);
            DrawRect(0, 0, width, height, UIColors.panel);
            for (int i = 0; i < items.Length; ++i) {
                items[i].Draw(SelectedIndex == i);
            }
            addEnvelopeButton.Draw();
        }
    }
}